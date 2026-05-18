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

            PauseSingleJobCommand = new RelayCommand(
                jobName =>
                {
                    if (jobName is string name)
                    {
                        var vm = GetJobVm(name);
                        if (vm == null || !vm.IsControllable) return;
                        _jobManager.PauseJob(name);
                        vm.SetPaused(true);
                    }
                });

            PlaySingleJobCommand = new RelayCommand(
                jobName =>
                {
                    if (jobName is string name)
                    {
                        var vm = GetJobVm(name);
                        if (vm == null || !vm.IsControllable) return;
                        _jobManager.PlayJob(name);
                        vm.SetPaused(false);
                    }
                });

            StopSingleJobCommand = new RelayCommand(
                jobName =>
                {
                    if (jobName is string name)
                    {
                        var vm = GetJobVm(name);
                        if (vm == null || !vm.IsControllable) return;
                        _jobManager.StopJob(name);
                        vm.SetStopped();
                    }
                });

            PauseAllCommand = new RelayCommand(_ =>
            {
                _jobManager.PauseAll();
                foreach (var vm in RunningJobs.Where(v => v.IsControllable))
                    vm.SetPaused(true);
            });

            PlayAllCommand = new RelayCommand(_ =>
            {
                _jobManager.PlayAll();
                foreach (var vm in RunningJobs.Where(v => v.IsControllable))
                    vm.SetPaused(false);
            });

            StopAllCommand = new RelayCommand(_ =>
            {
                _jobManager.StopAll();
                foreach (var vm in RunningJobs.Where(v => v.IsControllable))
                    vm.SetStopped();
            });
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
            CommandManager.InvalidateRequerySuggested();

            if (RunningJobs.Any(x => x.JobName == jobName)) return;

            var vm = new RunningJobViewModel(jobName, RefreshRunningJobs);
            RunningJobs.Add(vm);
            _jobManager.RegisterJobObserver(jobName, vm);
            _ = _jobManager.ExecuteJob(jobName);
        }

        public void StartMultipleJobs(IEnumerable<BackupJob> jobs)
        {
            _lastJobs = jobs.ToList();
            CommandManager.InvalidateRequerySuggested();

            foreach (var job in _lastJobs)
            {
                if (RunningJobs.Any(x => x.JobName == job.Name)) continue;

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
            => RunningJobs.FirstOrDefault(v => v.JobName == name);
    }
}