using System;

namespace EasySave.Model.Observers
{
    /// <summary>
    /// Represents a real-time snapshot of a backup job's progress.
    /// Sent to observers after each processed file.
    /// </summary>
    public class StatusSnapshot
    {
        public string JobName { get; }
        public DateTime Timestamp { get; }
        public string State { get; }

        public int TotalFiles { get; }
        public long TotalSize { get; }

        public int ProcessedFiles { get; }
        public long ProcessedSize { get; }

        public string? CurrentSourceFile { get; }
        public string? CurrentDestinationFile { get; }

        public StatusSnapshot(
            string jobName,
            DateTime timestamp,
            string state,
            int totalFiles,
            long totalSize,
            int processedFiles,
            long processedSize,
            string? currentSourceFile,
            string? currentDestinationFile)
        {
            JobName = jobName;
            Timestamp = timestamp;
            State = state;
            TotalFiles = totalFiles;
            TotalSize = totalSize;
            ProcessedFiles = processedFiles;
            ProcessedSize = processedSize;
            CurrentSourceFile = currentSourceFile;
            CurrentDestinationFile = currentDestinationFile;
        }
    }
}
