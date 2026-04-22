using EasySave.Localization;
using EasySave.Model.Backup;
using EasySave.Model.Config;

namespace EasySave.ViewModel
{
    /// <summary>
    /// Root ViewModel for the console application.
    /// Provides access to core services: job manager, localization, config.
    /// </summary>
    public class MainViewModel
    {
        public BackupJobManager JobManager { get; }
        public LocalizationService Localization { get; }
        public ConfigManager Config { get; }

        public MainViewModel(
            BackupJobManager jobManager,
            LocalizationService localization,
            ConfigManager config)
        {
            JobManager = jobManager;
            Localization = localization;
            Config = config;
        }

        /// <summary>
        /// Loads the configured language at startup.
        /// </summary>
        public void Initialize()
        {
            Localization.LoadLanguage(Config.Config.Language);
        }
    }
}
