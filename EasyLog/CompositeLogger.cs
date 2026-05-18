namespace EasyLog
{
    /// <summary>
    /// Routes log entries based on the user-configured LogStorageMode.
    /// The mode is read dynamically via a delegate so changes in Settings
    /// take effect immediately without rebuilding the logger chain.
    /// </summary>
    public class CompositeLogger : ILogger
    {
        private readonly ILogger _local;
        private readonly ILogger _central;
        private readonly Func<LogStorageMode> _getMode;

        public CompositeLogger(ILogger local, ILogger central, Func<LogStorageMode> getMode)
        {
            _local = local;
            _central = central;
            _getMode = getMode;
        }

        public void LogEntry(LogEntry entry)
        {
            var mode = _getMode();
            if (mode is LogStorageMode.LocalOnly or LogStorageMode.Both) _local.LogEntry(entry);
            if (mode is LogStorageMode.CentralizedOnly or LogStorageMode.Both) _central.LogEntry(entry);
        }
    }
}