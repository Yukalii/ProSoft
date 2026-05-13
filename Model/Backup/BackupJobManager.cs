using EasySave.Model.Backup;
using EasySave.Model.BusinessSoftware;
using EasySave.Model.Config;
using EasySave.Model.Logger;
using EasySave.Model.Observers;
using EasySave.Model.Storage;
using EasySave.Model.Strategies;

public class BackupJobManager
{
    private readonly string _jobsFilePath;
    private readonly AppConfig _config;
    private readonly IBusinessSoftwareManager _businessSoftware;

    // Shared only for loading/saving jobs
    private readonly IStorage _sharedStorage;
    private readonly ILogger _sharedLogger;

    // Supports multiple observers (UI + StatusTracker)
    private readonly List<IBackupObserver> _observers = new();

    // Tracks running jobs
    private readonly Dictionary<string, Task> _runningJobs = new();
    private readonly HashSet<string> _waitingJobs = new();
    private readonly Dictionary<string, JobControlToken> _controlTokens = new();

    public int? MaxJobs { get; set; } = null;
    public bool CanAddJob => MaxJobs == null || Jobs.Count < MaxJobs;

    public List<BackupJob> Jobs { get; private set; } = new();

    public BackupJobManager(
        string jobsFilePath,
        IStorage storage,
        ILogger logger,
        IBackupObserver statusObserver,
        AppConfig config,
        IBusinessSoftwareManager businessSoftware)
    {
        _jobsFilePath = jobsFilePath;
        _sharedStorage = storage;
        _sharedLogger = logger;
        _config = config;
        _businessSoftware = businessSoftware;

        // Add initial observer
        _observers.Add(statusObserver);

        LoadJobs();
    }

    //  Parallel execution support
    public bool IsJobRunning(string name)
        => _runningJobs.ContainsKey(name);

    public bool IsJobPaused(string name)
    {
        lock (_controlTokens)
        {
            return _controlTokens.TryGetValue(name, out var token) && token.IsPaused;
        }
    }
    public IReadOnlyDictionary<string, Task> RunningJobs => _runningJobs;

    public async Task ExecuteJob(string name)
    {
        lock (_waitingJobs)
        {
            if (_waitingJobs.Contains(name) || _runningJobs.ContainsKey(name))
                return;

            _waitingJobs.Add(name);
        }

        var controlToken = new JobControlToken();

        lock (_controlTokens)
        {
            _controlTokens[name] = controlToken;
        }

        try
        {
            // Wait for business software to close
            while (_businessSoftware.SoftwareIsRunning())
                await Task.Delay(2000);

            var job = Jobs.Find(j => j.Name == name);
            if (job == null)
                return;

            var storage = new LocalStorage();
            var logger = new JsonLogger(_config.LogDirectory);

            // Create a unique status file for this job
            string statusFile = Path.Combine(
                _config.StatusFilePath,
                $"{name}_status.json"
            );

            var statusTracker = new StatusTracker(statusFile);

            var strategy = CreateStrategy(job.Strategy.GetType().Name);

            // Create a fresh job instance for this execution
            var jobInstance = new BackupJob(
                job.Name,
                job.SourcePath,
                job.TargetPath,
                strategy,
                storage,
                logger,
                _config
            );

            // Attach all observers (UI observers)
            foreach (var obs in _observers)
                jobInstance.AttachObserver(obs);

            // Attach per-job StatusTracker
            jobInstance.AttachObserver(statusTracker);

            //  Run job in background
            var task = Task.Run(() =>
            {
                jobInstance.Execute(controlToken);
            });

            lock (_runningJobs)
            {
                _runningJobs[name] = task;
            }

            await task;

            lock (_runningJobs)
            {
                _runningJobs.Remove(name);
            }
        }
        finally
        {
            lock (_waitingJobs)
            {
                _waitingJobs.Remove(name);
            }
            lock (_controlTokens)
            {
                if (_controlTokens.TryGetValue(name, out var token))
                {
                    token.Dispose();
                    _controlTokens.Remove(name);
                }
            }
        }
    }

    public void PlayJob(string name)
    {
        lock (_controlTokens)
        {
            if (_controlTokens.TryGetValue(name, out var token))
                token.Play();
        }
    }

    public void PauseJob(string name)
    {
        lock (_controlTokens)
        {
            if (_controlTokens.TryGetValue(name, out var token))
                token.Pause();
        }
    }

    public void StopJob(string name)
    {
        lock (_controlTokens)
        {
            if (_controlTokens.TryGetValue(name, out var token))
                token.Stop();
        }
    }

    public void PlayAll()
    {
        lock (_controlTokens)
        {
            foreach (var token in _controlTokens.Values)
                token.Play();
        }
    }

    public void PauseAll()
    {
        lock (_controlTokens)
        {
            foreach (var token in _controlTokens.Values)
                token.Pause();
        }
    }

    public void StopAll()
    {
        lock (_controlTokens)
        {
            foreach (var token in _controlTokens.Values)
                token.Stop();
        }
    }

    //  Load / save jobs
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
                _sharedStorage,
                _sharedLogger,
                _config
            );

            // Attach all observers
            foreach (var obs in _observers)
                job.AttachObserver(obs);

            Jobs.Add(job);
        }
    }

    private void SaveJobs()
    {
        var jobDtos = Jobs.Select(job => new BackupJobDTO
        {
            Name = job.Name,
            SourcePath = job.SourcePath,
            TargetPath = job.TargetPath,
            StrategyType = job.Strategy.GetType().Name
        }).ToList();

        string json = JsonSerializer.Serialize(jobDtos, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_jobsFilePath, json);
    }

    //  Job management
    public void AddJob(string name, string source, string target, string strategyType)
    {
        if (!CanAddJob)
            throw new InvalidOperationException(
                $"Cannot add job: maximum of {MaxJobs} jobs reached."
            );

        if (Jobs.Any(j => j.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("JobAlreadyExists");

        var strategy = CreateStrategy(strategyType);

        var job = new BackupJob(name, source, target, strategy, _sharedStorage, _sharedLogger, _config);

        // Attach all observers
        foreach (var obs in _observers)
            job.AttachObserver(obs);

        Jobs.Add(job);
        SaveJobs();
    }

    public void DeleteJob(string name)
    {
        Jobs.RemoveAll(j => j.Name == name);
        SaveJobs();
    }

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
        _observers.Add(observer);

        foreach (var job in Jobs)
            job.AttachObserver(observer);
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
