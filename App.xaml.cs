using EasySave.Localisation;
using EasySave.Model.Backup;
using EasySave.Model.Config;
using EasySave.Model.Logger;
using EasySave.Model.Storage;
using EasySave.View;
using EasySave.ViewModel;
using EasySave.Model.Observers;
using EasySave.Model.BusinessSoftware;

namespace EasySave
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IBackupObserver statusTracker = new StatusTracker("statusTracker.json");

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var configManager = new ConfigManager(Path.Combine(baseDir, "config.json"));

            var localisation = new LocalisationService(Path.Combine(baseDir, "Languages"));
            localisation.LoadLanguage(configManager.Config.Language);

            var baseLogger = LoggerFactory.Resolve(
                configManager.Config.LogFormat,
                Path.Combine(baseDir, configManager.Config.LogDirectory)
            );
            var dynamicLogger = new DynamicLogger(baseLogger);
            var storage = new LocalStorage();
            var businessSoftware = new BusinessSoftwareManager(configManager);
            var jobManager = new BackupJobManager(
                Path.Combine(baseDir, "jobs.json"),
                storage,
                dynamicLogger,
                statusTracker,
                configManager.Config,
                businessSoftware
            );

            var mainVM = new MainViewModel(jobManager, localisation, configManager, dynamicLogger);
            mainVM.Initialize();

            new MainWindow { DataContext = mainVM }.Show();
        }
    }
}