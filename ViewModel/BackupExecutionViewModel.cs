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

        public ICommand StartSingleJobCommand { get; }
        public ICommand StartMultipleJobsCommand { get; }

        public ICommand PauseSingleJobCommand { get; }
        public ICommand PlaySingleJobCommand { get; }
        public ICommand StopSingleJobCommand { get; }

        public ICommand PauseAllCommand { get; }
        public ICommand PlayAllCommand { get; }
        public ICommand StopAllCommand { get; }

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

        public void Reset()
        {
            RunningJobs.Clear();
            GlobalTotalFiles = 0;
            GlobalProcessedFiles = 0;
            OnPropertyChanged(nameof(GlobalTotalFilesDisplay));
        }
        /*
        public void StartSingleJob(string jobName)
        {
            var job = _jobManager.Jobs.FirstOrDefault(j => j.Name == jobName);
            if (job == null) return;

            _lastJobs = new List<BackupJob> { job };

            var vm = new RunningJobViewModel(jobName, RefreshRunningJobs);
            RunningJobs.Add(vm);
            _jobManager.RegisterObserver(vm);
            _ = _jobManager.ExecuteJob(jobName);
        }*/

        public void StartSingleJob(string jobName)
        {
            if (RunningJobs.Any(x => x.JobName == jobName)) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var vm = new RunningJobViewModel(jobName, RefreshRunningJobs);
                RunningJobs.Add(vm);
                _jobManager.RegisterJobObserver(jobName, vm);
                _ = _jobManager.ExecuteJob(jobName);
            });
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

        /*
         public void StartMultipleJobs(IEnumerable<BackupJob> jobs)
        {
            foreach (var job in jobs)
            {
                StartSingleJob(job.Name);
            }
        }*/

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
}