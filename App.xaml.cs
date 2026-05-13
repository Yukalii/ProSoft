using System.Runtime.InteropServices;
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
        // ── Win32 console helpers ────────────────────────────────────────────
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        private static void AttachOrAllocConsole()
        {
            const int ATTACH_PARENT_PROCESS = -1;
            if (!AttachConsole(ATTACH_PARENT_PROCESS))
                AllocConsole();
        }

        // ── Entry point ──────────────────────────────────────────────────────
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Shared bootstrap
            //IBackupObserver statusTracker = new StatusTracker("statusTracker.json");
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
                null,
                configManager.Config,
                businessSoftware
            );

            // ── CLI mode ─────────────────────────────────────────────────────
            if (e.Args.Length > 0)
            {
                AttachOrAllocConsole();

                int exitCode = 0;
                try
                {
                    var indices = BackupJobArgsParser.Parse(e.Args[0]);
                    exitCode = await BackupCliRunner.RunAsync(jobManager, indices);
                }
                catch (FormatException ex)
                {
                    Console.Error.WriteLine($"[ERROR] Bad argument: {ex.Message}");
                    Console.Error.WriteLine("Usage:");
                    Console.Error.WriteLine("  EasySave.exe 1-3      run jobs 1 through 3");
                    Console.Error.WriteLine("  EasySave.exe \"1;3\"    run jobs 1 and 3");
                    exitCode = 2;
                }

                Shutdown(exitCode);
                return;
            }

            // ── GUI mode ──────────────────────────────────────────────────────
            var mainVM = new MainViewModel(jobManager, localisation, configManager, dynamicLogger);
            mainVM.Initialize();

            new MainWindow { DataContext = mainVM }.Show();
        }
    }
}