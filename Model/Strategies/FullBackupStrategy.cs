using EasySave.Model.Backup;
using EasySave.Model.Encryption;
using EasySave.Model.Logger;
using EasySave.Model.Observers;

namespace EasySave.Model.Strategies
{
    /// <summary>
    /// Full backup strategy with concurrency control:
    /// Only one large file (> threshold KB) may be processed at a time.
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        public async Task ExecuteAsync(BackupJobContext context)
        {
            var allFiles = context.Storage.EnumerateFiles(context.SourcePath).ToList();
            var priorityExts = context.Config.PriorityExtensions.Select(e => e.ToLowerInvariant()).ToHashSet();
            var priorityFiles = allFiles.Where(f => priorityExts.Contains(Path.GetExtension(f).ToLowerInvariant())).ToList();
            var normalFiles = allFiles.Where(f => !priorityExts.Contains(Path.GetExtension(f).ToLowerInvariant())).ToList();

            // Register priority count in the shared gate BEFORE iterating
            context.PriorityGate?.AddPendingPriority(priorityFiles.Count);

            // Process priority files first, then normal files
            var orderedFiles = priorityFiles.Concat(normalFiles).ToList();

            long totalSize = allFiles.Sum(f => context.Storage.GetFileInfo(f).Size);
            int totalFiles = allFiles.Count;
            long processedSize = 0;
            int processedFiles = 0;

            bool isPriority = false; // tracks which half we are in

            try
            {
                foreach (var sourceFile in orderedFiles)
                {
                    isPriority = priorityExts.Contains(
                        Path.GetExtension(sourceFile).ToLowerInvariant());

                    context.ControlToken?.WaitIfPaused();

                    // Gate: block non-priority file while any job has priority pending
                    if (!isPriority && context.PriorityGate != null)
                    {
                        await context.PriorityGate.WaitForClearanceAsync(context.ControlToken?.Token ?? CancellationToken.None);
                    }

                    if (context.BusinessSoftware != null)
                    {
                        while (context.BusinessSoftware.SoftwareIsRunning())
                        {
                            context.ControlToken?.Token.ThrowIfCancellationRequested();

                            Notify(context.Observers, new StatusSnapshot(
                                context.JobName, DateTime.Now, "Waiting",
                                totalFiles, totalSize, processedFiles, processedSize,
                                sourceFile, "Waiting for business software to close..."));

                            await Task.Delay(1000);
                        }
                    }
                    context.ControlToken?.Token.ThrowIfCancellationRequested();

                    string relativePath = Path.GetRelativePath(context.SourcePath, sourceFile);
                    string destinationFile = Path.Combine(context.TargetPath, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

                    var fileInfo = context.Storage.GetFileInfo(sourceFile);
                    bool isLarge = fileInfo.Size > (context.LargeFileThresholdKb * 1024);

                    // Debug.WriteLine($"File size: {fileInfo.Size} bytes, threshold: {context.LargeFileThresholdKb * 1024}");      // Defense test
                    if (isLarge)
                    {
                        // Debug.WriteLine($"[WAIT] Large file detected ({sourceFile}). Waiting for semaphore...");      // Defense test
                        await context.LargeFileSemaphore.WaitAsync(context.ControlToken?.Token ?? CancellationToken.None);
                        // Debug.WriteLine($"[ENTER] Semaphore acquired for large file: {sourceFile}");      // Defense test
                    }

                    try
                    {
                        var sw = Stopwatch.StartNew();
                        bool success = context.Storage.CopyFile(sourceFile, destinationFile,
                            context.ControlToken);
                        sw.Stop();

                        int encTime = (success && CryptoSoftInvoker.ShouldEncrypt(sourceFile, context.Config))
                            ? CryptoSoftInvoker.EncryptFile(destinationFile, context.Config) : 0;

                        context.Logger.LogEntry(new LogEntry { });

                        processedFiles++;
                        processedSize += fileInfo.Size;

                        Notify(context.Observers, new StatusSnapshot(
                            context.JobName, DateTime.Now, "Active",
                            totalFiles, totalSize, processedFiles, processedSize,
                            sourceFile, destinationFile));
                    }
                    finally
                    {
                        if (isLarge)
                        {
                            context.LargeFileSemaphore.Release();
                            // Debug.WriteLine($"[EXIT] Semaphore released for large file: {sourceFile}");      // Defense test
                        }
                        if (isPriority)
                            context.PriorityGate?.OnePriorityDone();
                    }
                }

                Notify(context.Observers, new StatusSnapshot(
                    context.JobName, DateTime.Now, "Inactive",
                    totalFiles, totalSize, processedFiles, processedSize, null, null));
            }
            catch (OperationCanceledException)
            {
                // If cancelled mid-priority-batch, drain remaining priority count
                // so other jobs are not permanently blocked
                int remaining = priorityFiles.Count - processedFiles; // rough estimate
                for (int i = 0; i < Math.Max(0, remaining); i++)
                    context.PriorityGate?.OnePriorityDone();

                Notify(context.Observers, new StatusSnapshot(
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