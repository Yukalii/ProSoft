using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Input;
using EasySave.Model.Backup;
using EasySave.Model.Observers;

namespace EasySave.ViewModel
{
    public class BackupExecutionViewModel : ViewModelBase
    {
        private readonly BackupJobManager _jobManager;

        public ObservableCollection<RunningJobViewModel> RunningJobs { get; }
            = new ObservableCollection<RunningJobViewModel>();

        //  Global progress
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

        //  Start jobs
        public void StartSingleJob(string jobName)
        {
            var vm = new RunningJobViewModel(jobName, RefreshRunningJobs);
            RunningJobs.Add(vm);

            _jobManager.RegisterObserver(vm);
            _ = _jobManager.ExecuteJob(jobName);
        }

        public void StartMultipleJobs(IEnumerable<BackupJob> jobs)
        {
            foreach (var job in jobs)
            {
                var vm = new RunningJobViewModel(job.Name, RefreshRunningJobs);
                RunningJobs.Add(vm);

                _jobManager.RegisterObserver(vm);
                _ = _jobManager.ExecuteJob(job.Name);
            }
        }


        //  Refresh global progress
        public void RefreshRunningJobs()
        {
            GlobalTotalFiles = RunningJobs.Sum(j => j.TotalFiles);
            GlobalProcessedFiles = RunningJobs.Sum(j => j.ProcessedFiles);

            OnPropertyChanged(nameof(GlobalTotalFilesDisplay));
        }
    }

    //  Per-job View-model
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

        // Called by StatusTracker
        public void OnJobUpdated(StatusSnapshot snapshot)
        {
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
