using EasySave.Model.Backup;
using EasySave.Model.Config;
using EasySave.Model.Logger;
using EasySave.Localisation;


namespace EasySave.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public BackupJobManager JobManager { get; }

        public JobListViewModel JobListVM { get; }
        public JobEditorViewModel JobEditorVM { get; }
        public BackupExecutionViewModel BackupExecutionVM { get; }
        public SettingsViewModel SettingsVM { get; }

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

        public MainViewModel(
            BackupJobManager jobManager,
            LocalisationService localisation,
            ConfigManager configManager,
            DynamicLogger dynamicLogger)
        {
            JobManager = jobManager;

            BackupExecutionVM = new BackupExecutionViewModel(jobManager);

            JobListVM = new JobListViewModel(
    jobManager,
    onRunJob: jobName =>
    {
        BackupExecutionVM.JobName = jobName;
        CurrentViewModel = BackupExecutionVM;
    },
    onJobDeleted: () => BackupExecutionVM.RefreshJobs(),
    onJobEdit: job => 
    {
        CurrentViewModel = new JobEditorViewModel(jobManager, () =>
        {
            JobListVM.RefreshJobs();
            JobListVM.SelectedJob = null;
            BackupExecutionVM.RefreshJobs();
            CurrentViewModel = JobListVM;
        }, job);
    }
);

            JobEditorVM = new JobEditorViewModel(jobManager, () =>
            {
                JobListVM.RefreshJobs();
                JobListVM.SelectedJob = null; 
                BackupExecutionVM.RefreshJobs();
                CurrentViewModel = JobListVM;
            });

            SettingsVM = new SettingsViewModel(localisation, configManager, dynamicLogger);

            ShowJobsCommand = new RelayCommand(_ =>
            {
                JobListVM.SelectedJob = null; 
                CurrentViewModel = JobListVM;
            });

            ShowExecutionCommand = new RelayCommand(_ => CurrentViewModel = BackupExecutionVM);
            ShowSettingsCommand = new RelayCommand(_ => CurrentViewModel = SettingsVM);
            ShowJobEditorCommand = new RelayCommand(_ => CurrentViewModel = JobEditorVM);

            CurrentViewModel = JobListVM;
        }

        public void Initialize()
        {
            JobManager.RegisterObserver(BackupExecutionVM);
        }
    }
}