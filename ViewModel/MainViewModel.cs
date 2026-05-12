using EasySave.Localisation;
using EasySave.Model.Backup;
using EasySave.Model.Config;
using EasySave.Model.Logger;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EasySave.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public BackupJobManager JobManager { get; }
        public LocalisationService Loc { get; }

        // --- ViewModels ---
        public JobListViewModel JobListVM { get; }
        public SettingsViewModel SettingsVM { get; }

        // One execution VM managing all running jobs
        public BackupExecutionViewModel BackupExecutionVM { get; }

        private ViewModelBase _currentViewModel = null!;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        // --- Navigation Commands ---
        public ICommand ShowJobsCommand { get; }
        public ICommand ShowExecutionCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowJobEditorCommand { get; }

        // --- Unified Execution Command ---
        public ICommand RunSelectedJobsCommand { get; }

        public MainViewModel(
            BackupJobManager jobManager,
            LocalisationService localisation,
            ConfigManager configManager,
            DynamicLogger dynamicLogger)
        {
            JobManager = jobManager;
            Loc = localisation;

            // One execution VM for the whole execution view
            BackupExecutionVM = new BackupExecutionViewModel(jobManager);

            // Job list with callbacks
            JobListVM = new JobListViewModel(
                jobManager,
                onRunJob: jobName =>
                {
                    // Row-level "Run" button → treat as single job selection
                    BackupExecutionVM.StartSingleJob(jobName);
                    CurrentViewModel = BackupExecutionVM;
                },
                onJobDeleted: () => BackupExecutionVM.RefreshRunningJobs(),
                onJobEdit: job =>
                {
                    CurrentViewModel = new JobEditorViewModel(jobManager, () =>
                    {
                        JobListVM.RefreshJobs();
                        JobListVM.SelectedJob = null;
                        BackupExecutionVM.RefreshRunningJobs();
                        CurrentViewModel = JobListVM;
                    }, job);
                }
            );

            SettingsVM = new SettingsViewModel(localisation, configManager, dynamicLogger);

            // Navigation
            ShowJobsCommand = new RelayCommand(_ =>
            {
                JobListVM.SelectedJob = null;
                CurrentViewModel = JobListVM;
            });

            ShowExecutionCommand = new RelayCommand(_ =>
            {
                CurrentViewModel = BackupExecutionVM;
            });

            ShowSettingsCommand = new RelayCommand(_ => CurrentViewModel = SettingsVM);

            ShowJobEditorCommand = new RelayCommand(_ =>
            {
                CurrentViewModel = new JobEditorViewModel(jobManager, () =>
                {
                    JobListVM.RefreshJobs();
                    CurrentViewModel = JobListVM;
                });
            });

            // Unified execution command
            RunSelectedJobsCommand = new RelayCommand(
                _ => RunSelectedJobs(),
                _ => JobListVM.SelectedJobs.Any()
            );

            CurrentViewModel = JobListVM;
        }

        public void Initialize()
        {
        }

        //  Execution logic

        private void RunSelectedJobs()
        {
            var selectedJobs = JobListVM.SelectedJobs.ToList();

            if (!selectedJobs.Any())
                return;

            if (selectedJobs.Count == 1)
            {
                BackupExecutionVM.StartSingleJob(selectedJobs[0].Name);
            }
            else
            {
                BackupExecutionVM.StartMultipleJobs(selectedJobs);
            }

            CurrentViewModel = BackupExecutionVM;
        }
    }
}
