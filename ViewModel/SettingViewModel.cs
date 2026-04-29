using EasySave.Model.Config;
using EasySave.Localisation;
using EasySave.Model.Logger;

namespace EasySave.ViewModel
{
    /// <summary>
    /// Handles saving of user preferences: language and log format.
    /// Applies changes immediately without requiring a restart.
    /// </summary>
    public class SettingsViewModel
    {
        private readonly ConfigManager _configManager;
        private readonly LocalisationService _localisation;
        private readonly DynamicLogger _dynamicLogger;

        public string SelectedLanguage
        {
            get => _configManager.Config.Language;
            set => _configManager.Config.Language = value;
        }

        public string SelectedLogFormat
        {
            get => _configManager.Config.LogFormat;
            set => _configManager.Config.LogFormat = value;
        }

        public string CryptoSoftKey
        {
            get => _configManager.Config.CryptoSoftKey;
            set => _configManager.Config.CryptoSoftKey = value;
        }

        public List<string> EncryptedExtensions
        {
            get => _configManager.Config.EncryptedExtensions;
            set => _configManager.Config.EncryptedExtensions = value;
        }

        public SettingsViewModel(
            LocalisationService localisation,
            ConfigManager configManager,
            DynamicLogger dynamicLogger)
        {
            _localisation = localisation;
            _configManager = configManager;
            _dynamicLogger = dynamicLogger;
        }

        /// <summary>
        /// Saves settings, reloads the language and swaps the logger if format changed.
        /// </summary>
        public void SaveSettings()
        {
            _configManager.Save();
            _localisation.LoadLanguage(_configManager.Config.Language);

            // Swap the active logger to match the new format
            ILogger newLogger = LoggerFactory.Resolve(
                _configManager.Config.LogFormat,
                _configManager.Config.LogDirectory
            );

            _dynamicLogger.SwapLogger(newLogger);
        }
    }
}