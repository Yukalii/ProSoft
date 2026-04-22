using System;

namespace EasySave.Model.Strategies
{
    public interface IBackupStrategy
    {
        /// <summary>
        /// Executes the backup logic for the given job.
        /// </summary>
        void Execute(BackupJob job);
    }
}
