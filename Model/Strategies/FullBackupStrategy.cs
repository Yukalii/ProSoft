using System;
using System.Collections.Generic;
using System.Text;

using System;
using System.Diagnostics;
using System.IO;
using EasySave.Model.Backup;
using EasySave.Model.Logging;
using EasySave.Model.Observers;
using EasySave.Model.Storage;

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

                Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

                var fileInfo = storage.GetFileInfo(sourceFile);

                var stopwatch = Stopwatch.StartNew();
                bool success = storage.CopyFile(sourceFile, destinationFile);
                stopwatch.Stop();

                long transferTime = success ? stopwatch.ElapsedMilliseconds : -1;

                // Log the action
                logger.LogEntry(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    JobName = context.JobName,
                    SourcePath = sourceFile,
                    DestinationPath = destinationFile,
                    FileSize = fileInfo.Size,
                    TransferTimeMs = transferTime
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
