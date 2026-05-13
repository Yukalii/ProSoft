namespace EasySave.Model.Logger
{
    public class JsonLogger : ILogger
    {
        private readonly string _logDirectory;

        private static readonly object _fileLock = new object();

        public JsonLogger(string logDirectory)
        {
            _logDirectory = logDirectory;

            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
        }

        public void LogEntry(LogEntry entry)
        {
            string fileName = $"log_{DateTime.Now:yyyy-MM-dd}.json";
            string filePath = Path.Combine(_logDirectory, fileName);

            string json = JsonSerializer.Serialize(entry, new JsonSerializerOptions
            {
                WriteIndented = false
            });
            lock (_fileLock)
            {
                File.AppendAllText(filePath, json + Environment.NewLine);
            }
        }
    }
}
