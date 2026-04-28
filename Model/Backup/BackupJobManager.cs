using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave.Model.Logger;
using EasySave.Model.Storage;
using EasySave.Model.Strategies;
using EasySave.Model.Observers;

namespace EasySave.Model.Backup
{
    /// <summary>
    /// Manages the list of backup jobs, handles persistence,
    /// and provides methods to execute jobs.
    /// </summary>
    public class BackupJobManager
    {
        private readonly string _jobsFilePath;
        private readonly IStorage _storage;
        private readonly ILogger _logger;
        private readonly IBackupObserver _statusObserver;

        /// <summary>
        /// Maximum number of backup jobs allowed. Null means no limit.
        /// </summary>
        public int? MaxJobs { get; set; } = 5;

        public bool CanAddJob => MaxJobs == null || Jobs.Count < MaxJobs;

        public List<BackupJob> Jobs { get; private set; } = new();

        public BackupJobManager(
            string jobsFilePath,
            IStorage storage,
            ILogger logger,
            IBackupObserver statusObserver)
        {
            _jobsFilePath = jobsFilePath;
            _storage = storage;
            _logger = logger;
            _statusObserver = statusObserver;

            LoadJobs();
        }

        /// <summary>
        /// Loads jobs from a JSON file.
        /// </summary>
        private void LoadJobs()
        {
            if (!File.Exists(_jobsFilePath))
            {
                Jobs = new List<BackupJob>();
                return;
            }

            string json = File.ReadAllText(_jobsFilePath);
            var jobDtos = JsonSerializer.Deserialize<List<BackupJobDTO>>(json)
                          ?? new List<BackupJobDTO>();

            Jobs = new List<BackupJob>();

            foreach (var dto in jobDtos)
            {
                var strategy = CreateStrategy(dto.StrategyType);

                var job = new BackupJob(
                    dto.Name,
                    dto.SourcePath,
                    dto.TargetPath,
                    strategy,
                    _storage,
                    _logger
                );

                job.AttachObserver(_statusObserver);
                Jobs.Add(job);
            }
        }

        /// <summary>
        /// Saves jobs to a JSON file.
        /// </summary>
        private void SaveJobs()
        {
            var jobDtos = new List<BackupJobDTO>();

            foreach (var job in Jobs)
            {
                jobDtos.Add(new BackupJobDTO
                {
                    Name = job.Name,
                    SourcePath = job.SourcePath,
                    TargetPath = job.TargetPath,
                    StrategyType = job.Strategy.GetType().Name
                });
            }

            string json = JsonSerializer.Serialize(jobDtos, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_jobsFilePath, json);
        }

        /// <summary>
        /// Adds a new backup job.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the job limit is reached.</exception>
        public void AddJob(string name, string source, string target, string strategyType)
        {
            if (!CanAddJob)
                throw new InvalidOperationException(
                    $"Cannot add job: maximum of {MaxJobs} jobs reached."
                );

            var strategy = CreateStrategy(strategyType);
            var job = new BackupJob(name, source, target, strategy, _storage, _logger);
            job.AttachObserver(_statusObserver);

            Jobs.Add(job);
            SaveJobs();
        }

        /// <summary>
        /// Deletes a job by name.
        /// </summary>
        public void DeleteJob(string name)
        {
            Jobs.RemoveAll(j => j.Name == name);
            SaveJobs();
        }

        /// <summary>
        /// Executes a job by name.
        /// </summary>
        public void ExecuteJob(string name)
        {
            var job = Jobs.Find(j => j.Name == name);
            job?.Execute();
        }

        /// <summary>
        /// Factory method to create a strategy from a string.
        /// </summary>
        private IBackupStrategy CreateStrategy(string type)
        {
            return type switch
            {
                "FullBackupStrategy" => new FullBackupStrategy(),
                "DifferentialBackupStrategy" => new DifferentialBackupStrategy(),
                _ => throw new InvalidOperationException($"Unknown strategy: {type}")
            };
        }
    }

    /// <summary>
    /// DTO used for saving/loading jobs.
    /// </summary>
    public class BackupJobDTO
    {
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public string StrategyType { get; set; }
    }
}
