using System.Text.Json;
using EasySave.Model.Config;
using EasySave.Model.Logger;
using EasySave.Model.Observers;
using EasySave.Model.Storage;
using EasySave.Model.Strategies;
using EasySave.Model.BusinessSoftware;

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
        private IBackupObserver _statusObserver;
        private readonly AppConfig _config;
        private readonly IBusinessSoftwareManager _businessSoftware;

        /// <summary>
        /// Maximum number of backup jobs allowed. Null means no limit.
        /// </summary>
        public int? MaxJobs { get; set; } = null;

        public bool CanAddJob => MaxJobs == null || Jobs.Count < MaxJobs;

        public List<BackupJob> Jobs { get; private set; } = new();

        private readonly HashSet<string> _waitingJobs = new HashSet<string>();

        public BackupJobManager(
            string jobsFilePath,
            IStorage storage,
            ILogger logger,
            IBackupObserver statusObserver,
            AppConfig config,
            IBusinessSoftwareManager businessSoftware)
        {
            _jobsFilePath = jobsFilePath;
            _storage = storage;
            _logger = logger;
            _statusObserver = statusObserver;
            _config = config;
            _businessSoftware = businessSoftware;

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
                    _logger,
                    _config
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
            var job = new BackupJob(name, source, target, strategy, _storage, _logger, _config);
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
        public async Task ExecuteJob(string name)
        {
            lock (_waitingJobs)
            {
                if (_waitingJobs.Contains(name))
                {
                    return;
                }
                _waitingJobs.Add(name);
            }
            try
            {
                while (_businessSoftware.SoftwareIsRunning())
                {
                    Debug.WriteLine("open");
                    await Task.Delay(2000);
                }
                Debug.WriteLine("close");
                var job = Jobs.Find(j => j.Name == name);
                job?.Execute();
            }
            finally
            {
                lock (_waitingJobs)
                {
                    _waitingJobs.Remove(name);
                }
            }
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
        public void RegisterObserver(IBackupObserver observer)
        {
            _statusObserver = observer;

            foreach (var job in Jobs)
                job.AttachObserver(observer);
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
