using System;
using System.IO;
using System.Diagnostics;
using EasySave.Model;

namespace EasySave.Model.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        private readonly IStorage _storage;
        private readonly ILogger _logger;

        public FullBackupStrategy(IStorage storage, ILogger logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public void Execute(BackupJob job)
        {
            var allFiles = _storage.GetFiles(job.SourcePath);
            long totalSize = 0;
            foreach (var file in allFiles)
                totalSize += new FileInfo(file).Length;

            int totalFiles = allFiles.Count;
            int processed = 0;
            long processedSize = 0;

            // Notify: job starting
            job.NotifyObservers(new StatusSnapshot
            {
                JobName = job.Name,
                State = "Active",
                TotalFiles = totalFiles,
                ProcessedFiles = processed,
                TotalSize = totalSize,
                ProcessedSize = processedSize,
                Timestamp = DateTime.Now
            });

            foreach (var srcFile in allFiles)
            {
                // Build the mirrored target path
                string relativePath = Path.GetRelativePath(job.SourcePath, srcFile);
                string tgtFile = Path.Combine(job.TargetPath, relativePath);

                // Ensure target sub-directory exists
                string? tgtDir = Path.GetDirectoryName(tgtFile);
                if (tgtDir != null && !Directory.Exists(tgtDir))
                    Directory.CreateDirectory(tgtDir);

                long fileSize = new FileInfo(srcFile).Length;
                long transferTime = -1;

                try
                {
                    var sw = Stopwatch.StartNew();
                    _storage.CopyFile(srcFile, tgtFile);
                    sw.Stop();
                    transferTime = sw.ElapsedMilliseconds;

                    processed++;
                    processedSize += fileSize;
                }
                catch (Exception ex)
                {
                    // transferTime stays negative to signal error (spec requirement)
                    _logger.Log(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = job.Name,
                        SourceFile = srcFile,
                        TargetFile = tgtFile,
                        FileSize = fileSize,
                        TransferTimeMs = transferTime,
                        Error = ex.Message
                    });
                    continue;
                }

                _logger.Log(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = job.Name,
                    SourceFile = srcFile,
                    TargetFile = tgtFile,
                    FileSize = fileSize,
                    TransferTimeMs = transferTime
                });

                job.NotifyObservers(new StatusSnapshot
                {
                    JobName = job.Name,
                    State = "Active",
                    TotalFiles = totalFiles,
                    ProcessedFiles = processed,
                    TotalSize = totalSize,
                    ProcessedSize = processedSize,
                    CurrentSourceFile = srcFile,
                    CurrentTargetFile = tgtFile,
                    Timestamp = DateTime.Now
                });
            }

            // Notify: job complete
            job.NotifyObservers(new StatusSnapshot
            {
                JobName = job.Name,
                State = "Inactive",
                TotalFiles = totalFiles,
                ProcessedFiles = processed,
                TotalSize = totalSize,
                ProcessedSize = processedSize,
                Timestamp = DateTime.Now
            });
        }
    }
}
