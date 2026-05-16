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
            var logger = context.Logger;
            var observers = context.Observers;
            var control = context.ControlToken;

            var allFiles = storage.EnumerateFiles(context.SourcePath).ToList();

            var filesToCopy = allFiles.Where(file =>
            {
                string rel = Path.GetRelativePath(context.SourcePath, file);
                string dest = Path.Combine(context.TargetPath, rel);
                var srcInfo = storage.GetFileInfo(file);
                var dstInfo = storage.GetFileInfo(dest);
                return !dstInfo.Exists
                    || srcInfo.LastModified > dstInfo.LastModified
                    || srcInfo.Size != dstInfo.Size;
            }).ToList();

            int totalFiles = filesToCopy.Count;
            long totalSize = filesToCopy.Sum(f => storage.GetFileInfo(f).Size);

            long processedSize = 0;
            int processedFiles = 0;

            try
            {
                foreach (var sourceFile in filesToCopy)
                {
                    control?.WaitIfPaused();

                    string relativePath = Path.GetRelativePath(context.SourcePath, sourceFile);
                    string destinationFile = Path.Combine(context.TargetPath, relativePath);

                    var sourceInfo = storage.GetFileInfo(sourceFile);

                    bool isLarge = sourceInfo.Size > context.LargeFileThresholdKb * 1024;

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
                        await context.LargeFileSemaphore.WaitAsync();

                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

                        var stopwatch = Stopwatch.StartNew();
                        bool success = storage.CopyFile(sourceFile, destinationFile, control);
                        stopwatch.Stop();

                        long transferTime = success ? stopwatch.ElapsedMilliseconds : -1;

                        int encryptionTime = 0;
                        if (success && CryptoSoftInvoker.ShouldEncrypt(sourceFile, context.Config))
                        {
                            encryptionTime = CryptoSoftInvoker.EncryptFile(destinationFile, context.Config);
                        }

                        logger.LogEntry(new LogEntry
                        {
                            Timestamp = DateTime.Now,
                            JobName = context.JobName,
                            SourcePath = sourceFile,
                            DestinationPath = destinationFile,
                            FileSize = sourceInfo.Size,
                            TransferTimeMs = transferTime,
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
                    }
                }

                Notify(observers, new StatusSnapshot(
                    context.JobName, DateTime.Now, "Inactive",
                    totalFiles, totalSize, processedFiles, processedSize,
                    null, null));
            }
            catch (OperationCanceledException)
            {
                Notify(observers, new StatusSnapshot(
                    context.JobName, DateTime.Now, "Stopped",
                    totalFiles, totalSize, processedFiles, processedSize,
                    null, null));
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
