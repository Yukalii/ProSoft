using System;

namespace EasySave.Model.Logging
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string JobName { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public long TransferTimeMs { get; set; }
    }
}
