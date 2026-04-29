namespace EasySave.Model.Observers
{
    /// <summary>
    /// Observer vide utilisé comme placeholder avant l'initialisation du ViewModel.
    /// </summary>
    public class NullBackupObserver : IBackupObserver
    {
        public void OnJobUpdated(StatusSnapshot snapshot) { } 
    }
}