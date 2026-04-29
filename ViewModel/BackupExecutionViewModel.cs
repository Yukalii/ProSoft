using EasySave.Model.Backup;
using EasySave.Model.Observers;

namespace EasySave.ViewModel
{
    public class BackupExecutionViewModel : ViewModelBase, IBackupObserver
    {
        private readonly BackupJobManager _jobManager;

        public ObservableCollection<BackupJob> AvailableJobs { get; } = new();

        private string _jobName = "";
        public string JobName
        {
            get => _jobName;
            set => SetProperty(ref _jobName, value);
        }

        private BackupJob? _selectedJob;
        public BackupJob? SelectedJob
        {
            get => _selectedJob;
            set
            {
                SetProperty(ref _selectedJob, value);
                _jobName = value?.Name ?? "";
                OnPropertyChanged(nameof(JobName));
                CommandManager.InvalidateRequerySuggested(); 
            }
        }

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
            private set
            {
                SetProperty(ref _totalFiles, value);
                OnPropertyChanged(nameof(TotalFilesDisplay));
            }
        }

        public int TotalFilesDisplay => TotalFiles > 0 ? TotalFiles : 1;

        private long _totalSize;
        public long TotalSize
        {
            get => _totalSize;
            private set => SetProperty(ref _totalSize, value);
        }

        private int _processedFiles;
        public int ProcessedFiles
        {
            get => _processedFiles;
            private set => SetProperty(ref _processedFiles, value);
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

        public ICommand StartJobCommand { get; }

        public BackupExecutionViewModel(BackupJobManager jobManager)
        {
            _jobManager = jobManager;
            RefreshJobs();

            StartJobCommand = new RelayCommand(
                _ => StartJob(_jobName),
                _ => !string.IsNullOrWhiteSpace(_jobName)
            );
        }

        public async void StartJob(string jobName)
        {
            JobName = jobName;
            await Task.Run(() => _jobManager.ExecuteJob(jobName));
        }

        public void RefreshJobs()
        {
            AvailableJobs.Clear();
            foreach (var job in _jobManager.Jobs)
                AvailableJobs.Add(job);
        }

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
            });
        }
    }
}