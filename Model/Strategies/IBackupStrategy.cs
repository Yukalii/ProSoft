using EasySave.Model.Backup;

namespace EasySave.Model.Strategies
{
    /// <summary>
    /// Defines the behavior of a backup strategy (Full or Differential).
    /// Each strategy determines which files must be copied and how.
    /// </summary>
    public interface IBackupStrategy
    {
        /// <summary>
        /// Executes the backup logic for a given job context.
        /// The context contains source, target, and job metadata.
        /// </summary>
        void Execute(BackupJobContext context);
    }
}
