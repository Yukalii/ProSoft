using System;
using System.Diagnostics;
using System.IO;
using EasySave.Model.Backup;
using EasySave.Model.Logger;
using EasySave.Model.Observers;
using EasySave.Model.Storage;

namespace EasySave.Model.Strategies
{
    /// <summary>
    /// Implements a differential backup strategy.
    /// Copies only files that are new or modified compared to the target.
    /// Logs each action and notifies observers of real-time progress.
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJobContext context)
        {
            var storage = context.Storage;
            var logger = context.Logger;
            var observers = context.Observers;

            var allFiles = storage.EnumerateFiles(context.SourcePath);
            long totalSize = 0;
            int totalFiles = 0;

            // Pre-calculate totals for progress reporting
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

                var sourceInfo = storage.GetFileInfo(sourceFile);
                var targetInfo = storage.GetFileInfo(destinationFile);

                bool mustCopy =
                    !targetInfo.Exists ||
                    sourceInfo.LastModified > targetInfo.LastModified ||
                    sourceInfo.Size != targetInfo.Size;

                if (mustCopy)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

                    var stopwatch = Stopwatch.StartNew();
                    bool success = storage.CopyFile(sourceFile, destinationFile);
                    stopwatch.Stop();

                    long transferTime = success ? stopwatch.ElapsedMilliseconds : -1;

                    logger.LogEntry(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        JobName = context.JobName,
                        SourcePath = sourceFile,
                        DestinationPath = destinationFile,
                        FileSize = sourceInfo.Size,
                        TransferTimeMs = transferTime
                    });
                }

                processedFiles++;
                processedSize += sourceInfo.Size;

                var snapshot = new StatusSnapshot(
                    context.JobName,
                    DateTime.Now,
                    "Active",
                    totalFiles,
                    totalSize,
                    processedFiles,
                    processedSize,
                    sourceFile,
                    mustCopy ? destinationFile : null
                );

                foreach (var obs in observers)
                    obs.OnJobUpdated(snapshot);
            }

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
