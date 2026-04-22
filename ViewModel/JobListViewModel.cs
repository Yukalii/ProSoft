using System.Collections.Generic;
using EasySave.Model.Backup;

namespace EasySave.ViewModel
{
    public class JobListViewModel
    {
        private readonly BackupJobManager _jobManager;

        public List<BackupJob> Jobs => _jobManager.Jobs;

        public JobListViewModel(BackupJobManager jobManager)
        {
            _jobManager = jobManager;
        }

        public void RefreshJobs()
        {
            // Nothing special for console, Jobs is always up to date
        }

        public void DeleteJob(string jobName)
        {
            _jobManager.DeleteJob(jobName);
        }
    }
}
