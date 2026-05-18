using EasySave.Model.Observers;
using EasySave.ViewModel;

public class RunningJobViewModel : ViewModelBase, IBackupObserver
{
    public string JobName { get; }

    private readonly Action _onUpdated;

    private string _state = "Pending";
    public string State
    {
        get => _state;
        private set
        {
            if (SetProperty(ref _state, value))
            {
                OnPropertyChanged(nameof(IsRunningAndNotPaused));
                OnPropertyChanged(nameof(IsControllable));
            }
        }
    }

    private int _totalFiles;
    public int TotalFiles
    {
        get => _totalFiles;
        private set => SetProperty(ref _totalFiles, value);
    }

    private int _processedFiles;
    public int ProcessedFiles
    {
        get => _processedFiles;
        private set => SetProperty(ref _processedFiles, value);
    }

    private long _totalSize;
    public long TotalSize
    {
        get => _totalSize;
        private set => SetProperty(ref _totalSize, value);
    }

    private long _processedSize;
    public long ProcessedSize
    {
        get => _processedSize;
        private set => SetProperty(ref _processedSize, value);
    }

    private string? _currentSourceFile;
    public string? CurrentSourceFile
    {
        get => _currentSourceFile;
        private set => SetProperty(ref _currentSourceFile, value);
    }

    private string? _currentDestinationFile;
    public string? CurrentDestinationFile
    {
        get => _currentDestinationFile;
        private set => SetProperty(ref _currentDestinationFile, value);
    }

    private bool _isPaused;
    public bool IsPaused
    {
        get => _isPaused;
        private set
        {
            if (SetProperty(ref _isPaused, value))
                OnPropertyChanged(nameof(IsRunningAndNotPaused));
        }
    }

    private bool _isStopped;
    public bool IsStopped
    {
        get => _isStopped;
        private set
        {
            if (SetProperty(ref _isStopped, value))
            {
                OnPropertyChanged(nameof(IsRunningAndNotPaused));
                OnPropertyChanged(nameof(IsControllable));
            }
        }
    }

    public bool IsControllable =>
        State != "Inactive" && State != "Stopped" && !IsStopped;

    public bool IsRunningAndNotPaused => State == "Active" && !IsPaused && !IsStopped;

    public RunningJobViewModel(string jobName, Action onUpdated)
    {
        JobName = jobName;
        _onUpdated = onUpdated;
    }

    public int Percentage
    {
        get
        {
            if (TotalFiles <= 0) return 0;
            return (int)Math.Round(ProcessedFiles * 100.0 / TotalFiles);
        }
    }

    public void SetPaused(bool paused)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (!IsControllable) return;

            IsPaused = paused;
            State = paused ? "Paused" : "Active";
            OnPropertyChanged(nameof(IsRunningAndNotPaused));
            _onUpdated?.Invoke();
        });
    }

    public void SetStopped()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsStopped = true;
            IsPaused = false;
            State = "Stopped";
            OnPropertyChanged(nameof(IsRunningAndNotPaused));
            _onUpdated?.Invoke();
        });
    }

    public void OnJobUpdated(StatusSnapshot snapshot)
    {
        if (snapshot.JobName != JobName) return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            TotalFiles = snapshot.TotalFiles;
            TotalSize = snapshot.TotalSize;
            ProcessedFiles = snapshot.ProcessedFiles;
            ProcessedSize = snapshot.ProcessedSize;
            CurrentSourceFile = snapshot.CurrentSourceFile;
            CurrentDestinationFile = snapshot.CurrentDestinationFile;

            switch (snapshot.State)
            {
                case "Stopped":
                    IsStopped = true;
                    IsPaused = false;
                    State = "Stopped";
                    break;

                case "Inactive":
                    IsStopped = false;
                    IsPaused = false;
                    State = "Inactive";
                    break;

                case "Paused":
                    if (!IsStopped)
                    {
                        IsPaused = true;
                        State = "Paused";
                    }
                    break;

                default:
                    if (!IsStopped)
                    {
                        if (!IsPaused || snapshot.State == "Waiting")
                            State = snapshot.State;
                    }
                    break;
            }

            OnPropertyChanged(nameof(Percentage));
            OnPropertyChanged(nameof(IsRunningAndNotPaused));
            OnPropertyChanged(nameof(IsControllable));
            _onUpdated?.Invoke();
        });
    }
}
