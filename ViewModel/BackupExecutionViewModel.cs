using EasySave.Model.Backup;
using EasySave.Model.Observers;

namespace EasySave.ViewModel
{
    public class BackupExecutionViewModel : ViewModelBase
    {
        private readonly BackupJobManager _jobManager;

        private readonly Action _onGoBack;
        private List<BackupJob> _lastJobs = new();

        public ObservableCollection<RunningJobViewModel> RunningJobs { get; }
            = new ObservableCollection<RunningJobViewModel>();

        private int _globalTotalFiles;
        public int GlobalTotalFiles
        {
            get => _globalTotalFiles;
            private set => SetProperty(ref _globalTotalFiles, value);
        }

        private int _globalProcessedFiles;
        public int GlobalProcessedFiles
        {
            get => _globalProcessedFiles;
            private set => SetProperty(ref _globalProcessedFiles, value);
        }

        public int GlobalTotalFilesDisplay => GlobalTotalFiles > 0 ? GlobalTotalFiles : 1;

        public ICommand GoBackCommand { get; }
        public ICommand RelaunchCommand { get; }

        public BackupExecutionViewModel(BackupJobManager jobManager, Action onGoBack)
        {
            _jobManager = jobManager;
            _onGoBack = onGoBack;

            GoBackCommand = new RelayCommand(_ => _onGoBack());

            RelaunchCommand = new RelayCommand(
                _ =>
                {
                    Reset();
                    if (_lastJobs.Count == 1)
                        StartSingleJob(_lastJobs[0].Name);
                    else
                        StartMultipleJobs(_lastJobs);
                },
                _ => _lastJobs.Count > 0
            );
        }

        public void Reset()
        {
            RunningJobs.Clear();
            GlobalTotalFiles = 0;
            GlobalProcessedFiles = 0;
            OnPropertyChanged(nameof(GlobalTotalFilesDisplay));
        }

        public void StartSingleJob(string jobName)
        {
            var job = _jobManager.Jobs.FirstOrDefault(j => j.Name == jobName);
            if (job == null) return;

            _lastJobs = new List<BackupJob> { job };

            var vm = new RunningJobViewModel(jobName, RefreshRunningJobs);
            RunningJobs.Add(vm);
            _jobManager.RegisterObserver(vm);
            _ = _jobManager.ExecuteJob(jobName);
        }

        public void StartMultipleJobs(IEnumerable<BackupJob> jobs)
        {
            _lastJobs = jobs.ToList();

            foreach (var job in _lastJobs)
            {
                var vm = new RunningJobViewModel(job.Name, RefreshRunningJobs);
                RunningJobs.Add(vm);
                _jobManager.RegisterObserver(vm);
                _ = _jobManager.ExecuteJob(job.Name);
            }
        }

        public void RefreshRunningJobs()
        {
            GlobalTotalFiles = RunningJobs.Sum(j => j.TotalFiles);
            GlobalProcessedFiles = RunningJobs.Sum(j => j.ProcessedFiles);
            OnPropertyChanged(nameof(GlobalTotalFilesDisplay));
        }
    }

    public class RunningJobViewModel : ViewModelBase, IBackupObserver
    {
        public string JobName { get; }
        private readonly Action _onUpdated;

        private string _state = "Inactive";
        public string State
        {
            get => _state;
            private set => SetProperty(ref _state, value);
        }

        private int _totalFiles;
        public int TotalFiles
        {
            get => _totalFiles;
            private set => SetProperty(ref _totalFiles, value);
        }

        private int _processedFiles;
        public int ProcessedFiles
        {
            get => _processedFiles;
            private set => SetProperty(ref _processedFiles, value);
        }

        private long _totalSize;
        public long TotalSize
        {
            get => _totalSize;
            private set => SetProperty(ref _totalSize, value);
        }

        private long _processedSize;
        public long ProcessedSize
        {
            get => _processedSize;
            private set => SetProperty(ref _processedSize, value);
        }

        private string? _currentSourceFile;
        public string? CurrentSourceFile
        {
            get => _currentSourceFile;
            private set => SetProperty(ref _currentSourceFile, value);
        }

        private string? _currentDestinationFile;
        public string? CurrentDestinationFile
        {
            get => _currentDestinationFile;
            private set => SetProperty(ref _currentDestinationFile, value);
        }

        public RunningJobViewModel(string jobName, Action onUpdated)
        {
            JobName = jobName;
            _onUpdated = onUpdated;
        }

        public int Percentage
        {
            get
            {
                if (TotalFiles <= 0) return 0;
                return (int)Math.Round(ProcessedFiles * 100.0 / TotalFiles);
            }
        }

        public void OnJobUpdated(StatusSnapshot snapshot)
        {
            if (snapshot.JobName != JobName)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                State = snapshot.State;
                TotalFiles = snapshot.TotalFiles;
                TotalSize = snapshot.TotalSize;
                ProcessedFiles = snapshot.ProcessedFiles;
                ProcessedSize = snapshot.ProcessedSize;
                CurrentSourceFile = snapshot.CurrentSourceFile;
                CurrentDestinationFile = snapshot.CurrentDestinationFile;

                OnPropertyChanged(nameof(Percentage));
                _onUpdated?.Invoke();
            });
        }
    }
}