using EasySave.Model.Config;
using EasySave.Model.Logging;
using EasySave.Localisation;

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