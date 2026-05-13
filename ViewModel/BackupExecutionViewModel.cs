using EasySave.Model.Backup;
using EasySave.Model.Observers;

namespace EasySave.ViewModel
{
    public class BackupExecutionViewModel : ViewModelBase
    {
        private readonly BackupJobManager _jobManager;

        private string? _lastSingleJob;
        private List<BackupJob>? _lastMultipleJobs;

        private Action? _navigateBack;

        public ICommand GoBackCommand { get; private set; }
        public ICommand RelaunchCommand { get; private set; }

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

        public ICommand StartSingleJobCommand { get; }
        public ICommand StartMultipleJobsCommand { get; }

        public BackupExecutionViewModel(BackupJobManager jobManager)
        {
            _jobManager = jobManager;

            GoBackCommand = new RelayCommand(_ => _navigateBack?.Invoke());
            RelaunchCommand = new RelayCommand(_ => Relaunch());

            StartSingleJobCommand = new RelayCommand(jobName =>
            {
                if (jobName is string name && !string.IsNullOrWhiteSpace(name))
                    StartSingleJob(name);
            });

            StartMultipleJobsCommand = new RelayCommand(jobs =>
            {
                if (jobs is IEnumerable<BackupJob> list)
                    StartMultipleJobs(list);
            });
        }

        public void SetNavigationCallbacks(Action navigateBack)
        {
            _navigateBack = navigateBack;
        }

        public void StartSingleJob(string jobName)
        {
            _lastSingleJob = jobName;
            _lastMultipleJobs = null;

            RunningJobs.Clear();
            GlobalTotalFiles = 0;
            GlobalProcessedFiles = 0;
            _jobManager.ClearObservers();

            var vm = new RunningJobViewModel(jobName, RefreshRunningJobs);
            RunningJobs.Add(vm);
            _jobManager.RegisterObserver(vm);
            _ = _jobManager.ExecuteJob(jobName);
        }

        public void StartMultipleJobs(IEnumerable<BackupJob> jobs)
        {
            _lastMultipleJobs = jobs.ToList();
            _lastSingleJob = null;

            RunningJobs.Clear();
            GlobalTotalFiles = 0;
            GlobalProcessedFiles = 0;
            _jobManager.ClearObservers();

            foreach (var job in _lastMultipleJobs)
            {
                var vm = new RunningJobViewModel(job.Name, RefreshRunningJobs);
                RunningJobs.Add(vm);
                _jobManager.RegisterObserver(vm);
            }

            foreach (var job in _lastMultipleJobs)
            {
                _ = _jobManager.ExecuteJob(job.Name);
            }
        }

        public void Relaunch()
        {
            if (_lastSingleJob != null)
                StartSingleJob(_lastSingleJob);
            else if (_lastMultipleJobs != null)
                StartMultipleJobs(_lastMultipleJobs);
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
        public string State { get => _state; private set => SetProperty(ref _state, value); }

        private int _totalFiles;
        public int TotalFiles { get => _totalFiles; private set => SetProperty(ref _totalFiles, value); }

        private int _processedFiles;
        public int ProcessedFiles { get => _processedFiles; private set => SetProperty(ref _processedFiles, value); }

        private long _totalSize;
        public long TotalSize { get => _totalSize; private set => SetProperty(ref _totalSize, value); }

        private long _processedSize;
        public long ProcessedSize { get => _processedSize; private set => SetProperty(ref _processedSize, value); }

        private string? _currentSourceFile;
        public string? CurrentSourceFile { get => _currentSourceFile; private set => SetProperty(ref _currentSourceFile, value); }

        private string? _currentDestinationFile;
        public string? CurrentDestinationFile { get => _currentDestinationFile; private set => SetProperty(ref _currentDestinationFile, value); }

        public int Percentage
        {
            get
            {
                if (TotalFiles <= 0) return 0;
                return (int)Math.Round(ProcessedFiles * 100.0 / TotalFiles);
            }
        }

        public RunningJobViewModel(string jobName, Action onUpdated)
        {
            JobName = jobName;
            _onUpdated = onUpdated;
        }

        public void OnJobUpdated(StatusSnapshot snapshot)
        {
            if (snapshot.JobName != JobName) return;

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