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
        private readonly localisationService _localisation;

        public ConsoleView(
            MainViewModel mainVM,
            JobListViewModel jobListVM,
            SettingsViewModel settingsVM,
            BackupExecutionViewModel executionVM,
            localisationService localisation)
        {
            _mainVM = mainVM;
            _jobListVM = jobListVM;
            _settingsVM = settingsVM;
            _executionVM = executionVM;
            _localisation = localisation;
        }

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
                                      $" ({_mainVM.JobManager.Jobs.Count}/{_mainVM.JobManager.MaxJobs} — " +
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

                editorVM.SaveCommand.Execute(null);
                _jobListVM.RefreshJobsCommand.Execute(null);

                Console.WriteLine("\n" + _localisation.Translate(LanguageKeys.JobCreated));
                Console.WriteLine("1. " + _localisation.Translate(LanguageKeys.AddJob));
                Console.WriteLine("2. " + _localisation.Translate(LanguageKeys.Back));
                Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

                if (Console.ReadLine() != "1") return;

                if (!_mainVM.JobManager.CanAddJob)
                {
                    Console.WriteLine(_localisation.Translate(LanguageKeys.MaxJobsReached));
                    Console.ReadKey();
                    return;
                }
            }
        }

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

                if (int.TryParse(input, out int choice) && choice >= 1 && choice <= _jobListVM.Jobs.Count)
                {
                    _jobListVM.SelectedJob = _jobListVM.Jobs[choice - 1];
                    _jobListVM.DeleteJobCommand.Execute(null);

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

                if (int.TryParse(input, out int choice) && choice >= 1 && choice <= _jobListVM.Jobs.Count)
                {
                    string jobName = _jobListVM.Jobs[choice - 1].Name;
                    _executionVM.StartJobCommand.Execute(jobName);

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

        private void ShowSettings()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== " + _localisation.Translate(LanguageKeys.Settings) + " ===");
                Console.WriteLine("1. English");
                Console.WriteLine("2. Français");
                Console.WriteLine("3. " + _localisation.Translate(LanguageKeys.Back));
                Console.Write(_localisation.Translate(LanguageKeys.Choice) + ": ");

                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        _settingsVM.SelectedLanguage = "en";
                        _settingsVM.SaveSettingsCommand.Execute(null);
                        Console.WriteLine(_localisation.Translate(LanguageKeys.SettingsSaved));
                        Console.ReadKey();
                        break;

                    case "2":
                        _settingsVM.SelectedLanguage = "fr";
                        _settingsVM.SaveSettingsCommand.Execute(null);
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
        }
    }
}
