using EasySave.Model.Backup;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EasySave.ViewModel
{
    public class JobListViewModel : ViewModelBase
    {
        private readonly BackupJobManager _jobManager;
        private readonly Action<string> _onRunJob;
        private readonly Action? _onJobDeleted;
        private readonly Action<BackupJob>? _onJobEdit;

        public ObservableCollection<BackupJob> Jobs { get; } = new();

        private BackupJob? _selectedJob;
        public BackupJob? SelectedJob
        {
            get => _selectedJob;
            set => SetProperty(ref _selectedJob, value);
        }

        public ICommand DeleteJobCommand { get; }
        public ICommand EditJobCommand { get; }
        public ICommand RunJobCommand { get; }

        public JobListViewModel(BackupJobManager jobManager, Action<string> onRunJob,
            Action? onJobDeleted = null, Action<BackupJob>? onJobEdit = null)
        {
            _jobManager = jobManager;
            _onRunJob = onRunJob;
            _onJobDeleted = onJobDeleted;
            _onJobEdit = onJobEdit;

            RefreshJobs();

            DeleteJobCommand = new RelayCommand(
                _ =>
                {
                    if (SelectedJob == null) return;
                    _jobManager.DeleteJob(SelectedJob.Name);
                    Jobs.Remove(SelectedJob);
                    SelectedJob = null;
                    _onJobDeleted?.Invoke();
                },
                _ => SelectedJob != null
            );

            EditJobCommand = new RelayCommand(
                _ => { if (SelectedJob != null) _onJobEdit?.Invoke(SelectedJob); },
                _ => SelectedJob != null
            );

            RunJobCommand = new RelayCommand(
                _ => { if (SelectedJob != null) _onRunJob(SelectedJob.Name); },
                _ => SelectedJob != null
            );
        }

        public void RefreshJobs()
        {
            Jobs.Clear();
            foreach (var job in _jobManager.Jobs)
                Jobs.Add(job);
        }
    }
}