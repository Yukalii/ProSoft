using EasySave.Model.Backup;
using EasySave.Model.Config;
using EasySave.Model.Logger;
using EasySave.Localisation;
using System.Windows.Input;

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

            JobListVM = new JobListViewModel(jobManager, jobName =>
            {
                BackupExecutionVM.JobName = jobName;
                CurrentViewModel = BackupExecutionVM;
            });

            JobEditorVM = new JobEditorViewModel(jobManager, () =>
            {
                JobListVM.RefreshJobs();
                CurrentViewModel = SettingsVM;
            });

            SettingsVM = new SettingsViewModel(localisation, configManager, dynamicLogger);

            ShowJobsCommand = new RelayCommand(_ => CurrentViewModel = JobListVM);
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