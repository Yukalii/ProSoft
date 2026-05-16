using EasySave.Model.BusinessSoftware;
using EasySave.Model.Config;
using EasySave.Model.Observers;
using EasySave.Model.Storage;
using EasySave.Model.Strategies;

namespace EasySave.Model.Backup
{
    /// <summary>
    /// Represents a backup job with its metadata and behavior.
    /// It delegates the actual backup logic to an IBackupStrategy
    /// and notifies observers of real-time status updates.
    /// </summary>
    public class BackupJob
    {
        public string Name { get; }
        public string SourcePath { get; }
        public string TargetPath { get; }
        public string StrategyName => Strategy.GetType().Name;
        public IBackupStrategy Strategy { get; }

        private readonly SemaphoreSlim _largeFileSemaphore;
        private readonly int _largeFileThresholdKb;
        private readonly IStorage _storage;
        private readonly ILogger _logger;
        private readonly List<IBackupObserver> _observers;
        private readonly AppConfig _config;
        private readonly IBusinessSoftwareManager _businessSoftware;
        private readonly PriorityGate? _priorityGate;

        public BackupJob(
            string name,
            string sourcePath,
            string targetPath,
            IBackupStrategy strategy,
            IStorage storage,
            ILogger logger,
            AppConfig config,
            SemaphoreSlim largeFileSemaphore,
            int largeFileThresholdKb,
            IBusinessSoftwareManager businessSoftware,
            PriorityGate? priorityGate = null)

        {
            Name = name;
            SourcePath = sourcePath;
            TargetPath = targetPath;
            Strategy = strategy;
            _storage = storage;
            _logger = logger;
            _observers = new List<IBackupObserver>();
            _config = config; 
            _largeFileSemaphore = largeFileSemaphore;
            _largeFileThresholdKb = largeFileThresholdKb;
            _businessSoftware = businessSoftware;
            _priorityGate = priorityGate;
        }

        /// <summary>
        /// Adds an observer that will receive real-time status updates.
        /// </summary>
        public void AttachObserver(IBackupObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        /// <summary>
        /// Removes an observer from the notification list.
        /// </summary>
        public void DetachObserver(IBackupObserver observer)
        {
            _observers.Remove(observer);
        }

        /// <summary>
        /// Notifies all observers with a status snapshot.
        /// </summary>
        public void NotifyObservers(StatusSnapshot snapshot)
        {
            foreach (var observer in _observers)
                observer.OnJobUpdated(snapshot);
        }

        /// <summary>
        /// Executes the backup job using the selected strategy.
        /// </summary>
        public Task ExecuteAsync() => Strategy.ExecuteAsync(BuildContext(null));
        public Task ExecuteAsync(JobControlToken controlToken)
            => Strategy.ExecuteAsync(BuildContext(controlToken));

        private BackupJobContext BuildContext(JobControlToken? controlToken)
        {
            return new BackupJobContext(
                Name,
                SourcePath,
                TargetPath,
                _storage,
                _logger,
                _observers,
                _config,
                _largeFileSemaphore,
                _largeFileThresholdKb,
                controlToken,
                _businessSoftware,
                _priorityGate);
        }
    }
}
