using EasySave.Model.Backup;

namespace EasySave.ViewModel
{
    public class JobListViewModel : ViewModelBase
    {
        private readonly BackupJobManager _jobManager;
        private readonly Action<string> _onRunJob;

        public List<BackupJob> Jobs => _jobManager.Jobs;

        private BackupJob? _selectedJob;
        public BackupJob? SelectedJob
        {
            get => _selectedJob;
            set => SetProperty(ref _selectedJob, value);
        }

        public ICommand DeleteJobCommand { get; }
        public ICommand RunJobCommand { get; }

        public JobListViewModel(BackupJobManager jobManager, Action<string> onRunJob)
        {
            _jobManager = jobManager;
            _onRunJob = onRunJob;

            DeleteJobCommand = new RelayCommand(
                _ => { if (SelectedJob != null) { DeleteJob(SelectedJob.Name); OnPropertyChanged(nameof(Jobs)); } },
                _ => SelectedJob != null
            );

            RunJobCommand = new RelayCommand(
                _ => { if (SelectedJob != null) _onRunJob(SelectedJob.Name); },
                _ => SelectedJob != null
            );
        }

        public void RefreshJobs() => OnPropertyChanged(nameof(Jobs));

        public void DeleteJob(string jobName)
        {
            _jobManager.DeleteJob(jobName);
            OnPropertyChanged(nameof(Jobs));
        }
    }
}