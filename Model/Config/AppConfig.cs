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
        public string LogFormat { get; set; } = "json"; // "json" or "xml"
        public int LargeFileThresholdKb { get; set; } = 800;
        public string StatusFilePath { get; set; } = "status.json";
        public string DefaultBackupLocation { get; set; } = "Backups";
        public string CryptoSoftPath { get; set; } = "CryptoSoft.exe";
        public string CryptoSoftKey { get; set; } = "secretKey";
        public List<string> EncryptedExtensions { get; set; } = [".pdf", ".txt", ".png"];

        public string BusinessApp { get; set; } = "CalculatorApp";

        /// <summary>
        /// Where log entries should be stored.
        /// Defaults to LocalOnly so existing deployments are unaffected.
        /// </summary>
        public LogStorageMode LogStorageMode { get; set; } = LogStorageMode.LocalOnly;

        /// <summary>
        /// Base URL of the Docker log server.
        /// Used when LogStorageMode is CentralizedOnly or Both.
        /// </summary>
        public string LogServerUrl { get; set; } = "http://localhost:8080";

        /// <summary>
        /// Creates a default configuration used when no config file exists.
        /// </summary>
        public static AppConfig CreateDefault()
        {
            return new AppConfig
            {
                Language = "en",
                LogDirectory = "Logs",
                LogFormat = "json",
                StatusFilePath = "status.json",
                DefaultBackupLocation = "Backups",
                CryptoSoftPath = "CryptoSoft.exe",
                CryptoSoftKey = "secretKey",
                EncryptedExtensions = [".pdf", ".txt", ".png"],
                LogStorageMode = LogStorageMode.LocalOnly,
                LogServerUrl = "http://localhost:8080"
            };
        }
    }
}
