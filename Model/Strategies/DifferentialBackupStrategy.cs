using EasySave.Model.Backup;
using EasySave.Model.Encryption;
using EasySave.Model.Logger;
using EasySave.Model.Observers;
using EasySave.Model.Storage;
using System.Diagnostics;

namespace EasySave.Model.Strategies
{
    /// <summary>
    /// Differential backup strategy with concurrency control:
    /// Only one large file (> threshold KB) may be processed at a time.
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public async Task ExecuteAsync(BackupJobContext context)
        {
            var storage = context.Storage;
            var observers = context.Observers;
            var control = context.ControlToken;

            var allFiles = storage.EnumerateFiles(context.SourcePath).ToList();

            // Pre-filter: only keep files that actually need to be copied
            var filesToCopy = allFiles.Where(file =>
            {
                string rel  = Path.GetRelativePath(context.SourcePath, file);
                string dest = Path.Combine(context.TargetPath, rel);
                var srcInfo = storage.GetFileInfo(file);
                var dstInfo = storage.GetFileInfo(dest);
                return !dstInfo.Exists
                    || srcInfo.LastModified > dstInfo.LastModified
                    || srcInfo.Size != dstInfo.Size;
            }).ToList();

            // Priority classification (applied on filesToCopy only)
            var priorityExts = context.Config.PriorityExtensions
                .Select(e => e.ToLowerInvariant())
                .ToHashSet();

            var priorityFiles = filesToCopy
                .Where(f => priorityExts.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToList();
            var normalFiles = filesToCopy
                .Where(f => !priorityExts.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToList();

            context.PriorityGate?.AddPendingPriority(priorityFiles.Count);

            // Process priority files first, then normal files
            var orderedFiles = priorityFiles.Concat(normalFiles).ToList();

            int  totalFiles    = orderedFiles.Count;
            long totalSize     = orderedFiles.Sum(f => storage.GetFileInfo(f).Size);
            long processedSize = 0;
            int  processedFiles = 0;

            try
            {
                foreach (var sourceFile in orderedFiles)
                {
                    control?.WaitIfPaused();

                    bool isPriority = priorityExts.Contains(
                        Path.GetExtension(sourceFile).ToLowerInvariant());

                    // Gate: block non-priority file while priority files are pending anywhere
                    if (!isPriority && context.PriorityGate != null)
                    {
                        await context.PriorityGate.WaitForClearanceAsync(
                            control?.Token ?? CancellationToken.None);
                    }

                    string relativePath    = Path.GetRelativePath(context.SourcePath, sourceFile);
                    string destinationFile = Path.Combine(context.TargetPath, relativePath);

                    var sourceInfo = storage.GetFileInfo(sourceFile);
                    bool isLarge   = sourceInfo.Size > context.LargeFileThresholdKb * 1024;

                    // Business Software management
                    if (context.BusinessSoftware != null)
                    {
                        while (context.BusinessSoftware.SoftwareIsRunning())
                        {
                            control?.Token.ThrowIfCancellationRequested();

                            Notify(observers, new StatusSnapshot(
                                context.JobName, DateTime.Now, "Waiting",
                                totalFiles, totalSize, processedFiles, processedSize,
                                sourceFile, "Paused: Business Software detected"));

                            await Task.Delay(1000);
                        }
                    }

                    // Limit concurrency for large files
                    if (isLarge)
                        await context.LargeFileSemaphore.WaitAsync(
                            control?.Token ?? CancellationToken.None);

                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

                        var stopwatch = Stopwatch.StartNew();
                        bool success  = storage.CopyFile(sourceFile, destinationFile, control);
                        stopwatch.Stop();

                        int encryptionTime = 0;
                        if (success && CryptoSoftInvoker.ShouldEncrypt(sourceFile, context.Config))
                            encryptionTime = CryptoSoftInvoker.EncryptFile(destinationFile, context.Config);

                        context.Logger.LogEntry(new LogEntry
                        {
                            Timestamp        = DateTime.Now,
                            JobName          = context.JobName,
                            SourcePath       = sourceFile,
                            DestinationPath  = destinationFile,
                            FileSize         = sourceInfo.Size,
                            TransferTimeMs   = success ? stopwatch.ElapsedMilliseconds : -1,
                            EncryptionTimeMs = encryptionTime
                        });

                        processedFiles++;
                        processedSize += sourceInfo.Size;

                        Notify(observers, new StatusSnapshot(
                            context.JobName, DateTime.Now, "Active",
                            totalFiles, totalSize, processedFiles, processedSize,
                            sourceFile, destinationFile));
                    }
                    finally
                    {
                        if (isLarge)
                            context.LargeFileSemaphore.Release();

                        // Always notify gate when a priority file is done (copied or skipped)
                        if (isPriority)
                            context.PriorityGate?.OnePriorityDone();
                    }
                }

                Notify(observers, new StatusSnapshot(
                    context.JobName, DateTime.Now, "Inactive",
                    totalFiles, totalSize, processedFiles, processedSize, null, null));
            }
            catch (OperationCanceledException)
            {
                Notify(observers, new StatusSnapshot(
                    context.JobName, DateTime.Now, "Stopped",
                    totalFiles, totalSize, processedFiles, processedSize, null, null));
                throw;
            }
        }

        private static void Notify(
            IEnumerable<IBackupObserver> observers, StatusSnapshot snapshot)
        {
            foreach (var obs in observers)
                obs.OnJobUpdated(snapshot);
        }
    }
}