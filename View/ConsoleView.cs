using EasySave.Localisation;
using EasySave.ViewModel;
using EasySave.Model.Backup;

namespace EasySave.View
{
    public class ConsoleView
    {
        private readonly MainViewModel _mainVM;
        private readonly JobListViewModel _jobListVM;
        private readonly SettingsViewModel _settingsVM;
        private readonly BackupExecutionViewModel _executionVM;
        private readonly LocalisationService _localisation;

        /// <summary>
        /// Initializes the view with all required ViewModels and the localisation service.
        /// </summary>
        public ConsoleView(
            MainViewModel mainVM,
            JobListViewModel jobListVM,
            SettingsViewModel settingsVM,
            BackupExecutionViewModel executionVM,
            LocalisationService localisation)
        {
            _mainVM = mainVM;
            _jobListVM = jobListVM;
            _settingsVM = settingsVM;
            _executionVM = executionVM;
            _localisation = localisation;
        }

        /// <summary>
        /// Starts the main application loop and displays the main menu.
        /// </summary>
        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(_localisation.Translate(LanguageKeys.AppTitle));
                Console.WriteLine("1. " + _localisation.Translate(LanguageKeys.ListJobs));
                Console.WriteLine("2. " + _localisation.Translate(LanguageKeys.Settings));
                Console.WriteLine("3. " + _localisation.Translate(LanguageKeys.Exit));
                Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ShowJobList();
                        break;

                    case "2":
                        ShowSettings();
                        break;

                    case "3":
                        return;

                    default:
                        Console.WriteLine(_localisation.Translate(LanguageKeys.InvalidChoice));
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Displays the list of backup jobs and provides options to add, delete, or run a job.
        /// Disables the add option when the maximum number of jobs is reached.
        /// </summary>
        private void ShowJobList()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== " + _localisation.Translate(LanguageKeys.ListJobs) + " ===");

                int index = 1;
                foreach (var job in _jobListVM.Jobs)
                {
                    Console.WriteLine($"{index}. {job.Name} ({job.Strategy.GetType().Name})");
                    index++;
                }

                bool canAdd = _mainVM.JobManager.CanAddJob;

                Console.WriteLine();
                if (canAdd)
                    Console.WriteLine("A. " + _localisation.Translate(LanguageKeys.AddJob));
                else
                    Console.WriteLine("A. " + _localisation.Translate(LanguageKeys.AddJob) +
                                      $" ({_mainVM.JobManager.Jobs.Count}/{_mainVM.JobManager.MaxJobs} - " +
                                      _localisation.Translate(LanguageKeys.MaxJobsReached) + ")");

                Console.WriteLine("D. " + _localisation.Translate(LanguageKeys.DeleteJob));
                Console.WriteLine("R. " + _localisation.Translate(LanguageKeys.RunJob));
                Console.WriteLine("B. " + _localisation.Translate(LanguageKeys.Back));
                Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

                string? input = Console.ReadLine()?.ToUpper();

                switch (input)
                {
                    case "A":
                        if (!canAdd)
                        {
                            Console.WriteLine(_localisation.Translate(LanguageKeys.MaxJobsReached));
                            Console.ReadKey();
                            break;
                        }
                        ShowJobEditor();
                        break;

                    case "D":
                        DeleteJob();
                        break;

                    case "R":
                        RunJob();
                        break;

                    case "B":
                        return;

                    default:
                        Console.WriteLine(_localisation.Translate(LanguageKeys.InvalidChoice));
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Displays the job creation form and saves the new job.
        /// The user can type "B" at any field to cancel and go back.
        /// Loops to allow creating multiple jobs consecutively.
        /// </summary>
        private void ShowJobEditor()
        {
            while (true)
            {
                var editorVM = new JobEditorViewModel(_mainVM.JobManager);

                Console.Clear();
                Console.WriteLine("=== " + _localisation.Translate(LanguageKeys.CreateJob) + " ===");
                Console.WriteLine("B. " + _localisation.Translate(LanguageKeys.Back));
                Console.WriteLine();

                Console.Write(_localisation.Translate(LanguageKeys.Name) + ": ");
                string? name = Console.ReadLine();
                if (name?.ToUpper() == "B") return;
                editorVM.Name = name ?? "";

                Console.Write(_localisation.Translate(LanguageKeys.SourcePath) + ": ");
                string? source = Console.ReadLine();
                if (source?.ToUpper() == "B") return;
                editorVM.SourcePath = source ?? "";

                Console.Write(_localisation.Translate(LanguageKeys.TargetPath) + ": ");
                string? target = Console.ReadLine();
                if (target?.ToUpper() == "B") return;
                editorVM.TargetPath = target ?? "";

                Console.WriteLine(_localisation.Translate(LanguageKeys.Strategy) + ":");
                Console.WriteLine("  1. Full");
                Console.WriteLine("  2. Differential");
                Console.WriteLine("  B. " + _localisation.Translate(LanguageKeys.Back));
                Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

                string? stratInput = Console.ReadLine();
                if (stratInput?.ToUpper() == "B") return;

                editorVM.SelectedStrategy = stratInput == "2"
                    ? "DifferentialBackupStrategy"
                    : "FullBackupStrategy";

                editorVM.Save();

                Console.WriteLine("\n" + _localisation.Translate(LanguageKeys.JobCreated));
                Console.WriteLine("1. " + _localisation.Translate(LanguageKeys.AddJob));
                Console.WriteLine("2. " + _localisation.Translate(LanguageKeys.Back));
                Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

                if (Console.ReadLine() != "1") return;

                // Re-check the job limit before allowing another creation
                if (!_mainVM.JobManager.CanAddJob)
                {
                    Console.WriteLine(_localisation.Translate(LanguageKeys.MaxJobsReached));
                    Console.ReadKey();
                    return;
                }
            }
        }

        /// <summary>
        /// Displays the list of jobs and prompts the user to select one for deletion.
        /// Loops to allow deleting multiple jobs consecutively.
        /// </summary>
        private void DeleteJob()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== " + _localisation.Translate(LanguageKeys.DeleteJob) + " ===");

                int index = 1;
                foreach (var job in _jobListVM.Jobs)
                {
                    Console.WriteLine($"  {index}. {job.Name}");
                    index++;
                }

                Console.WriteLine("  B. " + _localisation.Translate(LanguageKeys.Back));
                Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

                string? input = Console.ReadLine();
                if (input?.ToUpper() == "B") return;

                if (int.TryParse(input, out int choice) &&
                    choice >= 1 &&
                    choice <= _jobListVM.Jobs.Count)
                {
                    string jobName = _jobListVM.Jobs[choice - 1].Name;
                    _jobListVM.DeleteJob(jobName);

                    Console.WriteLine("\n" + _localisation.Translate(LanguageKeys.JobDeleted));
                    Console.WriteLine("1. " + _localisation.Translate(LanguageKeys.DeleteJob));
                    Console.WriteLine("2. " + _localisation.Translate(LanguageKeys.Back));
                    Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

                    if (Console.ReadLine() != "1") return;
                }
                else
                {
                    Console.WriteLine(_localisation.Translate(LanguageKeys.InvalidChoice));
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Displays the list of jobs and prompts the user to select one to execute.
        /// Shows real-time progress until the backup completes.
        /// </summary>
        private void RunJob()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== " + _localisation.Translate(LanguageKeys.RunJob) + " ===");

                int index = 1;
                foreach (var job in _jobListVM.Jobs)
                {
                    Console.WriteLine($"  {index}. {job.Name} ({job.Strategy.GetType().Name})");
                    index++;
                }

                Console.WriteLine("  B. " + _localisation.Translate(LanguageKeys.Back));
                Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

                string? input = Console.ReadLine();
                if (input?.ToUpper() == "B") return;

                if (int.TryParse(input, out int choice) &&
                    choice >= 1 &&
                    choice <= _jobListVM.Jobs.Count)
                {
                    string jobName = _jobListVM.Jobs[choice - 1].Name;
                    _executionVM.StartJob(jobName);

                    // Poll and display real-time progress until the job completes
                    while (_executionVM.State == "Active")
                    {
                        Console.Clear();
                        Console.WriteLine($"{_localisation.Translate(LanguageKeys.Job)}: {_executionVM.JobName}");
                        Console.WriteLine($"{_localisation.Translate(LanguageKeys.State)}: {_executionVM.State}");
                        Console.WriteLine($"{_localisation.Translate(LanguageKeys.Files)}: {_executionVM.ProcessedFiles}/{_executionVM.TotalFiles}");
                        Console.WriteLine($"{_localisation.Translate(LanguageKeys.Size)}: {_executionVM.ProcessedSize}/{_executionVM.TotalSize} bytes");
                        Console.WriteLine($"{_localisation.Translate(LanguageKeys.Current)}: {_executionVM.CurrentSourceFile}");
                        Thread.Sleep(200);
                    }

                    Console.WriteLine("\n" + _localisation.Translate(LanguageKeys.BackupFinished));
                    Console.WriteLine("1. " + _localisation.Translate(LanguageKeys.RunJob));
                    Console.WriteLine("2. " + _localisation.Translate(LanguageKeys.Back));
                    Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

                    if (Console.ReadLine() != "1") return;
                }
                else
                {
                    Console.WriteLine(_localisation.Translate(LanguageKeys.InvalidChoice));
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Displays the settings menu.
        /// Allows the user to change the application language and the log file format.
        /// Changes are saved and applied immediately.
        /// </summary>
        private void ShowSettings()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== " + _localisation.Translate(LanguageKeys.Settings) + " ===");

                // Show current values so the user knows what is active
                Console.WriteLine($"[{_localisation.Translate(LanguageKeys.Language)}: {_settingsVM.SelectedLanguage.ToUpper()}]");
                Console.WriteLine($"[{_localisation.Translate(LanguageKeys.LogFormat)}: {_settingsVM.SelectedLogFormat.ToUpper()}]");
                Console.WriteLine();

                Console.WriteLine("1. " + _localisation.Translate(LanguageKeys.Language));
                Console.WriteLine("2. " + _localisation.Translate(LanguageKeys.LogFormat));
                Console.WriteLine("3. " + _localisation.Translate(LanguageKeys.Back));
                Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ShowLanguageSettings();
                        break;

                    case "2":
                        ShowLogFormatSettings();
                        break;

                    case "3":
                        return;

                    default:
                        Console.WriteLine(_localisation.Translate(LanguageKeys.InvalidChoice));
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Sub-menu for changing the application language.
        /// </summary>
        private void ShowLanguageSettings()
        {
            Console.Clear();
            Console.WriteLine("=== " + _localisation.Translate(LanguageKeys.Language) + " ===");
            Console.WriteLine("1. English");
            Console.WriteLine("2. Français");
            Console.WriteLine("3. " + _localisation.Translate(LanguageKeys.Back));
            Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    _settingsVM.SelectedLanguage = "en";
                    _settingsVM.SaveSettings();
                    Console.WriteLine(_localisation.Translate(LanguageKeys.SettingsSaved));
                    Console.ReadKey();
                    break;

                case "2":
                    _settingsVM.SelectedLanguage = "fr";
                    _settingsVM.SaveSettings();
                    Console.WriteLine(_localisation.Translate(LanguageKeys.SettingsSaved));
                    Console.ReadKey();
                    break;

                case "3":
                    return;

                default:
                    Console.WriteLine(_localisation.Translate(LanguageKeys.InvalidChoice));
                    Console.ReadKey();
                    break;
            }
        }

        /// <summary>
        /// Sub-menu for changing the log file format between JSON and XML.
        /// The new format applies to all log entries written after the change.
        /// </summary>
        private void ShowLogFormatSettings()
        {
            Console.Clear();
            Console.WriteLine("=== " + _localisation.Translate(LanguageKeys.LogFormat) + " ===");
            Console.WriteLine($"1. {_localisation.Translate(LanguageKeys.LogFormatJson)}");
            Console.WriteLine($"2. {_localisation.Translate(LanguageKeys.LogFormatXml)}");
            Console.WriteLine("3. " + _localisation.Translate(LanguageKeys.Back));
            Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    _settingsVM.SelectedLogFormat = "json";
                    _settingsVM.SaveSettings();
                    Console.WriteLine(_localisation.Translate(LanguageKeys.LogFormatSaved));
                    Console.ReadKey();
                    break;

                case "2":
                    _settingsVM.SelectedLogFormat = "xml";
                    _settingsVM.SaveSettings();
                    Console.WriteLine(_localisation.Translate(LanguageKeys.LogFormatSaved));
                    Console.ReadKey();
                    break;

                case "3":
                    return;

                default:
                    Console.WriteLine(_localisation.Translate(LanguageKeys.InvalidChoice));
                    Console.ReadKey();
                    break;
            }
        }
    }
}