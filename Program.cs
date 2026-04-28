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

            // Storage
            IStorage storage = new LocalStorage();

            // Status tracking (observer)
            IBackupObserver statusTracker = new StatusTracker("statusTracker.json");

            // Config
            ConfigManager config = new ConfigManager("config.json");

            // Localisation
            string localisationPath = Path.Combine(
                Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
                "Localization", "Languages"
            );
            LocalisationService localisation = new LocalisationService(localisationPath);
            localisation.LoadLanguage(config.Config.Language);

            // Logging - DynamicLogger reads the format from config and can be swapped at runtime
            ILogger initialLogger = LoggerFactory.Resolve(
                config.Config.LogFormat,
                config.Config.LogDirectory
            );
            DynamicLogger dynamicLogger = new DynamicLogger(initialLogger);

            // Backup job manager
            BackupJobManager jobManager = new BackupJobManager(
                "jobs.json",
                storage,
                dynamicLogger,
                statusTracker
            );

            // === Initialize ViewModels ===

            MainViewModel mainVM = new MainViewModel(jobManager, localisation, config);
            JobListViewModel jobListVM = new JobListViewModel(jobManager);
            SettingsViewModel settingsVM = new SettingsViewModel(localisation, config, dynamicLogger);
            BackupExecutionViewModel execVM = new BackupExecutionViewModel(jobManager);

            // === Initialize View ===

            ConsoleView view = new ConsoleView(
                mainVM,
                jobListVM,
                settingsVM,
                execVM,
                localisation
            );

            // === CLI mode vs Interactive mode ===
            if (args.Length > 0)
            {
                // Command-line mode: "EasySave.exe 1-3" or "EasySave.exe 1;3"
                string arg = args[0];

                if (arg.Contains('-'))
                {
                    // "1-3" → run jobs 1, 2, 3 sequentially
                    var parts = arg.Split('-');
                    int from = int.Parse(parts[0]);
                    int to = int.Parse(parts[1]);
                    for (int i = from; i <= to; i++)
                        jobManager.ExecuteJob(i.ToString());
                }
                else if (arg.Contains(';'))
                {
                    // "1;3" → run jobs 1 and 3
                    foreach (var index in arg.Split(';'))
                        jobManager.ExecuteJob(index.Trim());
                }
                else
                {
                    // Single job: "EasySave.exe 2"
                    jobManager.ExecuteJob(arg);
                }
            }
            else
            {
                // Interactive mode: show the console menu
                view.Run();
            }
        }
    }
}