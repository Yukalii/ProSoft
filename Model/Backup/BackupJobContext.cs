using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.Generic;
using EasySave.Model.Logger;
using EasySave.Model.Observers;
using EasySave.Model.Storage;

namespace EasySave.Model.Backup
{
    /// <summary>
    /// Carries all the information required by a backup strategy
    /// to execute a backup operation.
    /// 
    /// This context object prevents strategies from depending directly
    /// on BackupJob, keeping the Strategy pattern clean and decoupled.
    /// </summary>
    public class BackupJobContext
    {
        /// <summary>
        /// Full path of the source directory.
        /// </summary>
        public string SourcePath { get; }

        /// <summary>
        /// Full path of the target directory.
        /// </summary>
        public string TargetPath { get; }

        /// <summary>
        /// Storage abstraction (local, network, external).
        /// </summary>
        public IStorage Storage { get; }

        /// <summary>
        /// Logger used to write daily log entries.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Observers that must be notified of real-time status updates.
        /// </summary>
        public IReadOnlyList<IBackupObserver> Observers { get; }

        /// <summary>
        /// Name of the backup job (used for logging and status).
        /// </summary>
        public string JobName { get; }

        public BackupJobContext(
            string jobName,
            string sourcePath,
            string targetPath,
            IStorage storage,
            ILogger logger,
            IReadOnlyList<IBackupObserver> observers)
        {
            JobName = jobName;
            SourcePath = sourcePath;
            TargetPath = targetPath;
            Storage = storage;
            Logger = logger;
            Observers = observers;
        }
    }
}
