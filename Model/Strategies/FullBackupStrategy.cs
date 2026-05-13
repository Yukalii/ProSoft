using EasySave.Model.Backup;
using EasySave.Model.Encryption;
using EasySave.Model.Logger;
using EasySave.Model.Observers;
using System.Diagnostics;
using System.Linq;

namespace EasySave.Model.Strategies
{
    /// <summary>
    /// Implements a full backup strategy.
    /// Copies all files from the source directory to the target directory,
    /// logs each action, and notifies observers of real-time progress.
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJobContext context)
        {
            var storage = context.Storage;
            var logger = context.Logger;
            var observers = context.Observers;
            var control = context.ControlToken;

            var allFiles = storage.EnumerateFiles(context.SourcePath).ToList();
            long totalSize = 0;

            foreach (var f in allFiles)
                totalSize += storage.GetFileInfo(f).Size;
            int totalFiles = allFiles.Count;

            long processedSize = 0;
            int processedFiles = 0;

            try
            {
                foreach (var sourceFile in allFiles)
                {
                    control?.WaitIfPaused();

                    string relativePath = Path.GetRelativePath(context.SourcePath, sourceFile);
                    string destinationFile = Path.Combine(context.TargetPath, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

                    var fileInfo = storage.GetFileInfo(sourceFile);

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
                        FileSize = fileInfo.Size,
                        TransferTimeMs = transferTime,
                        EncryptionTimeMs = encryptionTime
                    });

                    processedFiles++;
                    processedSize += fileInfo.Size;

                    Notify(observers, new StatusSnapshot(
                        context.JobName, DateTime.Now, "Active",
                        totalFiles, totalSize, processedFiles, processedSize,
                        sourceFile, destinationFile));
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