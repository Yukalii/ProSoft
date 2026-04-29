using EasySave.Localisation;
using EasySave.Model.Backup;
using EasySave.Model.Config;

namespace EasySave.ViewModel
{
    /// <summary>
    /// Root ViewModel for the console application.
    /// Provides access to core services: job manager, localisation, config.
    /// </summary>
    public class MainViewModel
    {
        public BackupJobManager JobManager { get; }
        public LocalisationService Localisation { get; }
        public ConfigManager Config { get; }

        public MainViewModel(
            BackupJobManager jobManager,
            LocalisationService localisation,
            ConfigManager config)
        {
            JobManager = jobManager;
            Localisation = localisation;
            Config = config;
        }

        /// <summary>
        /// Loads the configured language at startup.
        /// </summary>
        public void Initialize()
        {
            Localisation.LoadLanguage(Config.Config.Language);
        }
    }
}
