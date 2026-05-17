using System.Xml.Serialization;

namespace EasyLog
{
    [XmlRoot("LogEntry")]
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string JobName { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public long TransferTimeMs { get; set; }
        public int EncryptionTimeMs { get; set; }
        public string MachineName { get; set; } = Environment.MachineName;
        public string UserName { get; set; } = Environment.UserName;
    }
}
