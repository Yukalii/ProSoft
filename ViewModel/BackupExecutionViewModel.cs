using EasySave.Model.Backup;
using EasySave.Model.Observers;

namespace EasySave.ViewModel
{
    public class BackupExecutionViewModel : IBackupObserver
    {
        private readonly BackupJobManager _jobManager;

        public string JobName { get; private set; } = "";
        public string State { get; private set; } = "Inactive";

        public int TotalFiles { get; private set; }
        public long TotalSize { get; private set; }

        public int ProcessedFiles { get; private set; }
        public long ProcessedSize { get; private set; }

        public string? CurrentSourceFile { get; private set; }
        public string? CurrentDestinationFile { get; private set; }

        public BackupExecutionViewModel(BackupJobManager jobManager)
        {
            _jobManager = jobManager;
        }

        public void StartJob(string jobName)
        {
            JobName = jobName;
            _jobManager.ExecuteJob(jobName);
        }

        public void OnJobUpdated(StatusSnapshot snapshot)
        {
            State = snapshot.State;
            TotalFiles = snapshot.TotalFiles;
            TotalSize = snapshot.TotalSize;
            ProcessedFiles = snapshot.ProcessedFiles;
            ProcessedSize = snapshot.ProcessedSize;
            CurrentSourceFile = snapshot.CurrentSourceFile;
            CurrentDestinationFile = snapshot.CurrentDestinationFile;
        }
    }
}
