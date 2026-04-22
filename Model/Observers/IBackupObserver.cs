namespace EasySave.Model.Observers
{
    /// <summary>
    /// Observer interface for receiving real-time backup status updates.
    /// Implementations react to status snapshots sent by BackupJob.
    /// </summary>
    public interface IBackupObserver
    {
        /// <summary>
        /// Called whenever the backup job produces a new status snapshot.
        /// </summary>
        void OnJobUpdated(StatusSnapshot snapshot);
    }
}
