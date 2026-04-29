using EasySave.Model.Backup;
using EasySave.Model.Observers;
using System.Windows;
using System.Windows.Input;

namespace EasySave.ViewModel
{
    public class BackupExecutionViewModel : ViewModelBase, IBackupObserver
    {
        private readonly BackupJobManager _jobManager;

        private string _jobName = "";
        public string JobName
        {
            get => _jobName;
            set => SetProperty(ref _jobName, value); 

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
            StartJobCommand = new RelayCommand(
                _ => StartJob(JobName),
                _ => !string.IsNullOrWhiteSpace(JobName)
            );
        }

        public async void StartJob(string jobName)
        {
            JobName = jobName;
            await Task.Run(() => _jobManager.ExecuteJob(jobName));
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