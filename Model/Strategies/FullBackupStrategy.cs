using EasySave.Model.Backup;
using EasySave.Model.Encryption;
using EasySave.Model.Logger;
using EasySave.Model.Observers;
using System.Diagnostics;
using System.Linq;

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
            long totalSize = allFiles.Sum(f => context.Storage.GetFileInfo(f).Size);
            int totalFiles = allFiles.Count;
            long processedSize = 0;
            int processedFiles = 0;

            try
            {
                foreach (var sourceFile in allFiles)
                {
                    context.ControlToken?.WaitIfPaused();
                    context.ControlToken?.Token.ThrowIfCancellationRequested();

                    string relativePath = Path.GetRelativePath(context.SourcePath, sourceFile);
                    string destinationFile = Path.Combine(context.TargetPath, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

                    var fileInfo = context.Storage.GetFileInfo(sourceFile);
                    // Conversion KB en Bytes pour la comparaison
                    bool isLarge = fileInfo.Size > (context.LargeFileThresholdKb * 1024);

                    Debug.WriteLine($"{fileInfo.Size}");
                    if (isLarge)
                    {
                        Debug.WriteLine("Wait");
                        await context.LargeFileSemaphore.WaitAsync(context.ControlToken?.Token ?? CancellationToken.None);
                        Debug.WriteLine("Enter");
                    }

                    try
                    {
                        var sw = Stopwatch.StartNew();
                        bool success = context.Storage.CopyFile(sourceFile, destinationFile, context.ControlToken);
                        sw.Stop();

                        int encTime = (success && CryptoSoftInvoker.ShouldEncrypt(sourceFile, context.Config))
                            ? CryptoSoftInvoker.EncryptFile(destinationFile, context.Config) : 0;

                        context.Logger.LogEntry(new LogEntry { });

                        processedFiles++;
                        processedSize += fileInfo.Size;

                        // UN SEUL APPEL de notification
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
                            Debug.WriteLine("Exit");
                        }
                    }
                }

                Notify(context.Observers, new StatusSnapshot(context.JobName, DateTime.Now, "Inactive", totalFiles, totalSize, processedFiles, processedSize, null, null));
            }
            catch (OperationCanceledException)
            {
                Notify(context.Observers, new StatusSnapshot(context.JobName, DateTime.Now, "Stopped", totalFiles, totalSize, processedFiles, processedSize, null, null));
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