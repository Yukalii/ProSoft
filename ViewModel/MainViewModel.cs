using EasySave.Localisation;
using EasySave.Model.Config;

namespace EasySave.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public BackupJobManager JobManager { get; }
        public LocalisationService Loc { get; }

        public JobListViewModel JobListVM { get; }
        public SettingsViewModel SettingsVM { get; }
        public BackupExecutionViewModel BackupExecutionVM { get; }

        private ViewModelBase _currentViewModel = null!;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand ShowJobsCommand { get; }
        public ICommand ShowExecutionCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowJobEditorCommand { get; }
        public ICommand RunSelectedJobsCommand { get; }

        public MainViewModel(
            BackupJobManager jobManager,
            LocalisationService localisation,
            ConfigManager configManager,
            DynamicLogger dynamicLogger)
        {
            JobManager = jobManager;
            Loc = localisation;

            BackupExecutionVM = new BackupExecutionViewModel(
    jobManager,
    onGoBack: () =>
    {
        JobListVM.RefreshJobs();
        CurrentViewModel = JobListVM;
    }
);

            JobListVM = new JobListViewModel(
                jobManager,
                onRunJob: jobName =>
                {
                    BackupExecutionVM.Reset();
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

            ShowJobsCommand = new RelayCommand(_ =>
            {
                JobListVM.SelectedJob = null;
                CurrentViewModel = JobListVM;
            });

            ShowExecutionCommand = new RelayCommand(_ => CurrentViewModel = BackupExecutionVM);
            ShowSettingsCommand = new RelayCommand(_ => CurrentViewModel = SettingsVM);

            ShowJobEditorCommand = new RelayCommand(_ =>
            {
                CurrentViewModel = new JobEditorViewModel(jobManager, () =>
                {
                    JobListVM.RefreshJobs();
                    CurrentViewModel = JobListVM;
                });
            });

            RunSelectedJobsCommand = new RelayCommand(
                _ => RunSelectedJobs(),
                _ => JobListVM.SelectedJobs.Any()
            );

            CurrentViewModel = JobListVM;
        }

        public void Initialize() { }

        private void RunSelectedJobs()
        {
            var selectedJobs = JobListVM.SelectedJobs.ToList();
            if (!selectedJobs.Any()) return;

            BackupExecutionVM.Reset();

            if (selectedJobs.Count == 1)
                BackupExecutionVM.StartSingleJob(selectedJobs[0].Name);
            else
                BackupExecutionVM.StartMultipleJobs(selectedJobs);

            CurrentViewModel = BackupExecutionVM;
        }
    }
}