namespace EasySave.Model.Observers
{
    /// <summary>
    /// Observer clear and empty used as a placeholder before the ViewModel initialization.
    /// </summary>
    public class NullBackupObserver : IBackupObserver
    {
        public void OnJobUpdated(StatusSnapshot snapshot) { } 
    }
}