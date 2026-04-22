using EasySave.Localisation;
using EasySave.Model.Backup;
using EasySave.Model.Config;
using EasySave.Model.Logging;
using EasySave.Model.Observers;
using EasySave.Model.Storage;
using EasySave.View;
using EasySave.ViewModel;

namespace EasySave
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // === Initialize core services ===

            // Logging
            ILogger logger = new JsonLogger("logs");

            // Storage
            IStorage storage = new LocalStorage();

            // Status tracking (observer)
            IBackupObserver statusTracker = new StatusTracker("statusTracker.json");

            // Config
            ConfigManager config = new ConfigManager("config.json");

            // localisation
            //string localisationPath = Path.Combine(AppContext.BaseDirectory, "localisation"); //for the bin/Debug/net10.0 folder
            string localisationPath = Path.Combine(
    Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
    "localisation", "Languages"
);
            localisationService localisation = new localisationService(localisationPath);
            localisation.LoadLanguage(config.Config.Language);

            // Backup job manager
            BackupJobManager jobManager = new BackupJobManager(
                "jobs.json",
                storage,
                logger,
                statusTracker
            );

            // === Initialize ViewModels ===

            MainViewModel mainVM = new MainViewModel(jobManager, localisation, config);
            JobListViewModel jobListVM = new JobListViewModel(jobManager);
            SettingsViewModel settingsVM = new SettingsViewModel(localisation, config);
            BackupExecutionViewModel executionVM = new BackupExecutionViewModel(jobManager);

            // === Initialize View ===

            ConsoleView view = new ConsoleView(
                mainVM,
                jobListVM,
                settingsVM,
                executionVM,
                localisation
            );

            // === Start the application ===

            view.Run();
        }
    }
}
