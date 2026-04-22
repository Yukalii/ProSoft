using System;
using System.IO;
using System.Text.Json;

namespace EasySave.Model.Observers
{
    /// <summary>
    /// Observer that writes real-time backup status to a JSON file.
    /// The file is overwritten on each update to always reflect
    /// the most recent state of all running jobs.
    /// </summary>
    public class StatusTracker : IBackupObserver
    {
        private readonly string _statusFilePath;

        public StatusTracker(string statusFilePath)
        {
            _statusFilePath = statusFilePath;

            // Ensure directory exists
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

            File.WriteAllText(_statusFilePath, json);
        }
    }
}
