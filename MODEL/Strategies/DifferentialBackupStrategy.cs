using System;
using System.IO;
using System.Diagnostics;
using EasySave.Model;

namespace EasySave.Model.Strategies
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        private readonly IStorage _storage;
        private readonly ILogger _logger;

        public DifferentialBackupStrategy(IStorage storage, ILogger logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public void Execute(BackupJob job)
        {
            var allFiles = _storage.GetFiles(job.SourcePath);

            // --- Filter: only files absent from target or newer than target copy ---
            var filesToCopy = new System.Collections.Generic.List<string>();
            long totalSize = 0;

            foreach (var srcFile in allFiles)
            {
                string relativePath = Path.GetRelativePath(job.SourcePath, srcFile);
                string tgtFile = Path.Combine(job.TargetPath, relativePath);

                bool needsCopy = !File.Exists(tgtFile)
                    || File.GetLastWriteTimeUtc(srcFile) > File.GetLastWriteTimeUtc(tgtFile);

                if (needsCopy)
                {
                    filesToCopy.Add(srcFile);
                    totalSize += new FileInfo(srcFile).Length;
                }
            }

            int totalFiles = filesToCopy.Count;
            int processed = 0;
            long processedSize = 0;

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

            foreach (var srcFile in filesToCopy)
            {
                string relativePath = Path.GetRelativePath(job.SourcePath, srcFile);
                string tgtFile = Path.Combine(job.TargetPath, relativePath);

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
