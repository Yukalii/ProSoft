namespace EasySave.Model.Logger
{
    /// <summary>
    /// A proxy logger that delegates to an inner logger instance.
    /// Allows swapping the log format at runtime without restarting.
    /// </summary>
    public class DynamicLogger : ILogger
    {
        private ILogger _inner;

        public DynamicLogger(ILogger initial)
        {
            _inner = initial;
        }

        /// <summary>
        /// Replaces the active logger with a new implementation.
        /// All subsequent log entries will use the new format.
        /// </summary>
        public void SwapLogger(ILogger newLogger)
        {
            _inner = newLogger;
        }

        public void LogEntry(LogEntry entry)
        {
            _inner.LogEntry(entry);
        }
    }
}