using EasySave.Model.Backup;
using EasySave.Model.BusinessSoftware;
using EasySave.Model.Config;
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
    private readonly Dictionary<string, List<IBackupObserver>> _jobObservers = new();

    // Tracks running jobs
    private readonly Dictionary<string, Task> _runningJobs = new();
    private readonly HashSet<string> _waitingJobs = new();
    private readonly Dictionary<string, JobControlToken> _controlTokens = new();

    private readonly SemaphoreSlim _largeFileSemaphore = new(1, 1);
    public int LargeFileThresholdKb => _config.LargeFileThresholdKb;

    public int? MaxJobs { get; set; } = null;
    public bool CanAddJob => MaxJobs == null || Jobs.Count < MaxJobs;

    public List<BackupJob> Jobs { get; private set; } = new();

    public IReadOnlyDictionary<string, Task> RunningJobs => _runningJobs;

    public BackupJobManager(
        string jobsFilePath,
        IStorage storage,
        ILogger logger,
        IBackupObserver? statusObserver,
        AppConfig config,
        IBusinessSoftwareManager businessSoftware)
    {
        _jobsFilePath = jobsFilePath;
        _sharedStorage = storage;
        _sharedLogger = logger;
        _config = config;
        _businessSoftware = businessSoftware;

        // Add initial observer
        if (statusObserver != null)
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

    public void RegisterJobObserver(string jobName, IBackupObserver observer)
    {
        lock (_jobObservers)
        {
            if (!_jobObservers.TryGetValue(jobName, out var list))
            {
                list = new List<IBackupObserver>();
                _jobObservers[jobName] = list;
            }
            if (!list.Contains(observer))
                list.Add(observer);
        }
    }
    
    public void RegisterObserver(IBackupObserver observer)
    {
        lock (_observers)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }
    }

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

        _ = Task.Run(async () =>
        {
            try
            {
                // Wait for business software to close
                while (_businessSoftware.SoftwareIsRunning())
                {
                    controlToken.Token.ThrowIfCancellationRequested();
                    await Task.Delay(2000, controlToken.Token);
                }

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

                var strategy = CreateStrategy(job.Strategy.GetType().Name);

                // Create a fresh job instance for this execution
                var jobInstance = new BackupJob(
                    job.Name,
                    job.SourcePath,
                    job.TargetPath,
                    strategy,
                    storage,
                    logger,
                    _config,
                    _largeFileSemaphore,
                    LargeFileThresholdKb,
                    _businessSoftware
                );

            var statusTracker = new StatusTracker(statusFile);

            foreach (var obs in _observers)
                jobInstance.AttachObserver(obs);

                lock (_jobObservers)
                    if (_jobObservers.TryGetValue(name, out var dedicated))
                        foreach (var obs in dedicated)
                            jobInstance.AttachObserver(obs);

                jobInstance.AttachObserver(statusTracker);

                //  Run job in background
                var task = jobInstance.ExecuteAsync(controlToken);

                lock (_runningJobs)
                {
                    _runningJobs[name] = task;
                }

                await task;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Debug.WriteLine($"Error in {name}: {ex.Message}"); }
            finally
            {
                lock (_waitingJobs)
                {
                    _waitingJobs.Remove(name);
                }
                lock (_jobObservers)
                {
                    _jobObservers.Remove(name);
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
        });
        await Task.CompletedTask;
    }


    //Individual job control
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

    // All jobs control
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

    // Job Management
    public void AddJob(string name, string source, string target, string strategyType)
    {
        if (!CanAddJob)
            throw new InvalidOperationException($"Maximum of {MaxJobs} jobs reached.");
        if (Jobs.Any(j => j.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("JobAlreadyExists");

        var strategy = CreateStrategy(strategyType);

        var job = new BackupJob(
            name, 
            source, 
            target, 
            strategy, 
            _sharedStorage, 
            _sharedLogger, 
            _config,
            _largeFileSemaphore,
            LargeFileThresholdKb,
            _businessSoftware);

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

    //  Load / save jobs
    private void LoadJobs()
    {
        if (!File.Exists(_jobsFilePath))
        {
            Jobs = new();
            return;
        }

        var jobDtos = JsonSerializer.Deserialize<List<BackupJobDTO>>(
                       File.ReadAllText(_jobsFilePath))
                   ?? new List<BackupJobDTO>();

        Jobs = jobDtos.Select(dto => new BackupJob(
            dto.Name, 
            dto.SourcePath, 
            dto.TargetPath,
            CreateStrategy(dto.StrategyType),
            _sharedStorage, 
            _sharedLogger, 
            _config, 
            _largeFileSemaphore, 
            LargeFileThresholdKb, 
            _businessSoftware)).ToList();
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

        File.WriteAllText(_jobsFilePath,
            JsonSerializer.Serialize(jobDtos, new JsonSerializerOptions { WriteIndented = true }));
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

    public class BackupJobDTO
    {
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public string StrategyType { get; set; }
    }
}
