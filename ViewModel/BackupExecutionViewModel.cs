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

        public ICommand PauseSingleJobCommand { get; }
        public ICommand PlaySingleJobCommand { get; }
        public ICommand StopSingleJobCommand { get; }

        public ICommand PauseAllCommand { get; }
        public ICommand PlayAllCommand { get; }
        public ICommand StopAllCommand { get; }

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

            PauseSingleJobCommand = new RelayCommand(jobName =>
            {
                if (jobName is string name)
                {
                    _jobManager.PauseJob(name);
                    GetJobVm(name)?.SetPaused(true);
                }
            });

            PlaySingleJobCommand = new RelayCommand(jobName =>
            {
                if (jobName is string name)
                {
                    _jobManager.PlayJob(name);
                    GetJobVm(name)?.SetPaused(false);
                }
            });

            StopSingleJobCommand = new RelayCommand(jobName =>
            {
                if (jobName is string name)
                {
                    _jobManager.StopJob(name);
                    GetJobVm(name)?.SetStopped();
                }
            });

            PauseAllCommand = new RelayCommand(_ =>
            {
                _jobManager.PauseAll();
                foreach (var vm in RunningJobs)
                    vm.SetPaused(true);
            });

            PlayAllCommand = new RelayCommand(_ =>
            {
                _jobManager.PlayAll();
                foreach (var vm in RunningJobs)
                    vm.SetPaused(false);
            });

            StopAllCommand = new RelayCommand(_ =>
            {
                _jobManager.StopAll();
            });
        }

        public void StartSingleJob(string jobName)
        {
            if (RunningJobs.Any(x => x.JobName == jobName)) return;

            var vm = new RunningJobViewModel(jobName, RefreshRunningJobs);
            RunningJobs.Add(vm);

            _jobManager.RegisterJobObserver(jobName, vm);
            _ = _jobManager.ExecuteJob(jobName);
        }

        public void StartMultipleJobs(IEnumerable<BackupJob> jobs)
        {
            foreach (var job in jobs)
            {
                if (RunningJobs.Any(x => x.JobName == job.Name))
                    continue;

                var vm = new RunningJobViewModel(job.Name, RefreshRunningJobs);
                RunningJobs.Add(vm);

                _jobManager.RegisterJobObserver(job.Name, vm);
                _ = _jobManager.ExecuteJob(job.Name);
            }
        }

        public void RefreshRunningJobs()
        {
            GlobalTotalFiles = RunningJobs.Sum(j => j.TotalFiles);
            GlobalProcessedFiles = RunningJobs.Sum(j => j.ProcessedFiles);

            OnPropertyChanged(nameof(GlobalTotalFilesDisplay));
        }

        private RunningJobViewModel? GetJobVm(string name)
        {
            return RunningJobs.FirstOrDefault(v => v.JobName == name);
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

        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            private set
            {
                if (SetProperty(ref _isPaused, value))
                    OnPropertyChanged(nameof(IsRunningAndNotPaused));
            }
        }

        private bool _isStopped;
        public bool IsStopped
        {
            get => _isStopped;
            private set => SetProperty(ref _isStopped, value);
        }

        public bool IsRunningAndNotPaused => State == "Active" && !IsPaused && !IsStopped;

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


        public void SetPaused(bool paused)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsPaused = paused;
                if (paused)
                    State = paused ? "Paused" : (IsStopped ? "Stopped" : "Active");
            });
        }

        public void SetStopped()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsStopped = true;
                IsPaused = false;
                State = "Stopped";
                OnPropertyChanged(nameof(IsRunningAndNotPaused));
            });
        }

        public void OnJobUpdated(StatusSnapshot snapshot)
        {
            if (snapshot.JobName != JobName) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!IsPaused && !IsStopped)
                    State = snapshot.State;

                TotalFiles = snapshot.TotalFiles;
                TotalSize = snapshot.TotalSize;
                ProcessedFiles = snapshot.ProcessedFiles;
                ProcessedSize = snapshot.ProcessedSize;
                CurrentSourceFile = snapshot.CurrentSourceFile;
                CurrentDestinationFile = snapshot.CurrentDestinationFile;

                if (snapshot.State == "Stopped")
                    SetStopped();

                if (snapshot.State == "Inactive")
                {
                    IsPaused = false;
                    IsStopped = false;
                }

                OnPropertyChanged(nameof(Percentage));
                OnPropertyChanged(nameof(IsRunningAndNotPaused));
                _onUpdated?.Invoke();
            });
        }
    }
}
