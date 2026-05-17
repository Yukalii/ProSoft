using System.Text.Json;

namespace EasySave.Model.Observers
{
    /// <summary>
    /// Observer that writes real-time backup status to a JSON file.
    /// Thread-safe: a static lock and an atomic temp-file swap prevent
    /// IOException when multiple jobs write concurrently.
    /// </summary>
    public class StatusTracker : IBackupObserver
    {
        private readonly string _statusFilePath;

        // One lock object shared across ALL StatusTracker instances so that
        // jobs writing to different per-job files never race on the same path.
        // If you prefer per-file locking, make this an instance field instead.
        private static readonly object _writeLock = new object();

        public StatusTracker(string statusFilePath)
        {
            _statusFilePath = statusFilePath;

            var directory = Path.GetDirectoryName(_statusFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public void OnJobUpdated(StatusSnapshot snapshot)
        {
            string json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // Write to a temp file next to the real file, then rename atomically.
            // This guarantees the status file is never half-written or locked
            // when another job tries to read or write it at the same time.
            string tempPath = _statusFilePath + ".tmp";

            lock (_writeLock)
            {
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, _statusFilePath, overwrite: true);
            }
        }
    }
}
