using EasySave.Model.Backup;

namespace EasySave.ViewModel
{
    public class JobEditorViewModel
    {
        private readonly BackupJobManager _jobManager;

        public string Name { get; set; } = "";
        public string SourcePath { get; set; } = "";
        public string TargetPath { get; set; } = "";
        public string SelectedStrategy { get; set; } = "Full";

        public JobEditorViewModel(BackupJobManager jobManager)
        {
            _jobManager = jobManager;
        }

        public void Save()
        {
            _jobManager.AddJob(Name, SourcePath, TargetPath, SelectedStrategy);
        }
    }
}
