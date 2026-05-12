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

        // All jobs displayed in the list
        public ObservableCollection<JobListItemViewModel> Jobs { get; } = new();

        // Multi‑selection support
        public ObservableCollection<BackupJob> SelectedJobs { get; }
            = new ObservableCollection<BackupJob>();

        private JobListItemViewModel? _selectedJob;

        private void OnSelectionChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public JobListItemViewModel? SelectedJob
        {
            get => _selectedJob;
            set => SetProperty(ref _selectedJob, value);
        }

        public ICommand DeleteJobCommand { get; }
        public ICommand EditJobCommand { get; }
        public ICommand RunJobCommand { get; }

        public JobListViewModel(
            BackupJobManager jobManager,
            Action<string> onRunJob,
            Action? onJobDeleted = null,
            Action<BackupJob>? onJobEdit = null)
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

                    _jobManager.DeleteJob(SelectedJob.Job.Name);
                    Jobs.Remove(SelectedJob);
                    SelectedJob = null;

                    _onJobDeleted?.Invoke();
                },
                _ => SelectedJob != null
            );

            EditJobCommand = new RelayCommand(
                _ =>
                {
                    if (SelectedJob != null)
                        _onJobEdit?.Invoke(SelectedJob.Job);
                },
                _ => SelectedJob != null
            );

            RunJobCommand = new RelayCommand(
                _ =>
                {
                    foreach (var job in SelectedJobs)
                        _onRunJob(job.Name);
                },
                _ => SelectedJobs.Any()
            );
        }

        //  Multi-selection logic
        public void ToggleJobSelection(JobListItemViewModel item, bool isSelected)
        {
            if (isSelected)
            {
                if (!SelectedJobs.Contains(item.Job))
                    SelectedJobs.Add(item.Job);
            }
            else
            {
                SelectedJobs.Remove(item.Job);
            }

            OnSelectionChanged();
        }

        //  Refresh job list
        public void RefreshJobs()
        {
            Jobs.Clear();
            SelectedJobs.Clear();

            foreach (var job in _jobManager.Jobs)
            {
                var item = new JobListItemViewModel(job, OnSelectionChanged, this);
                Jobs.Add(item);
            }
        }
    }

    //  Wrapper for each job row (checkbox + job)
    public class JobListItemViewModel : ViewModelBase
    {
        private readonly Action _onSelectionChanged;
        private readonly JobListViewModel _parent;

        public JobListItemViewModel(BackupJob job, Action onSelectionChanged, JobListViewModel parent)
        {
            Job = job;
            _onSelectionChanged = onSelectionChanged;
            _parent = parent;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    _parent.ToggleJobSelection(this, value);
                    _onSelectionChanged?.Invoke();
                }
            }
        }

        public BackupJob Job { get; }
    }
}
