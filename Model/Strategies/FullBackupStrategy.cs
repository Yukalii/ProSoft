using EasySave.Model.Backup;
using EasySave.Model.Encryption;
using EasySave.Model.Logger;
using EasySave.Model.Observers;

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

            var allFiles = storage.EnumerateFiles(context.SourcePath);
            long totalSize = 0;
            int totalFiles = 0;

            foreach (var file in allFiles)
            {
                var info = storage.GetFileInfo(file);
                totalSize += info.Size;
                totalFiles++;
            }

            long processedSize = 0;
            int processedFiles = 0;

            foreach (var sourceFile in storage.EnumerateFiles(context.SourcePath))
            {
                string relativePath = Path.GetRelativePath(context.SourcePath, sourceFile);
                string destinationFile = Path.Combine(context.TargetPath, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

                var fileInfo = storage.GetFileInfo(sourceFile);

                var stopwatch = Stopwatch.StartNew();
                bool success = storage.CopyFile(sourceFile, destinationFile);
                stopwatch.Stop();

                long transferTime = success ? stopwatch.ElapsedMilliseconds : -1;

                // Encrypt backup if needed
                int encryptionTime = 0;
                if (success && CryptoSoftInvoker.ShouldEncrypt(sourceFile, context.Config))
                {
                    encryptionTime = CryptoSoftInvoker.EncryptFile(destinationFile, context.Config);
                }

                // Log the action
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

                // Update progress
                processedFiles++;
                processedSize += fileInfo.Size;

                // Notify observers
                var snapshot = new StatusSnapshot(
                    context.JobName,
                    DateTime.Now,
                    "Active",
                    totalFiles,
                    totalSize,
                    processedFiles,
                    processedSize,
                    sourceFile,
                    destinationFile
                );

                foreach (var obs in observers)
                    obs.OnJobUpdated(snapshot);
            }

            // Final inactive status
            var finalSnapshot = new StatusSnapshot(
                context.JobName,
                DateTime.Now,
                "Inactive",
                totalFiles,
                totalSize,
                processedFiles,
                processedSize,
                null,
                null
            );

            foreach (var obs in observers)
                obs.OnJobUpdated(finalSnapshot);
        }
    }
}
