using System;

namespace EasySave.Model.Config
{
    /// <summary>
    /// Represents all global application settings.
    /// Stored in a JSON file and loaded by ConfigManager.
    /// </summary>
    public class AppConfig
    {
        public string Language { get; set; } = "en";
        public string LogDirectory { get; set; } = "Logs";
        public string StatusFilePath { get; set; } = "status.json";
        public string DefaultBackupLocation { get; set; } = "Backups";

        /// <summary>
        /// Creates a default configuration used when no config file exists.
        /// </summary>
        public static AppConfig CreateDefault()
        {
            return new AppConfig
            {
                Language = "en",
                LogDirectory = "Logs",
                StatusFilePath = "status.json",
                DefaultBackupLocation = "Backups"
            };
        }
    }
}
