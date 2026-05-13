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
}
