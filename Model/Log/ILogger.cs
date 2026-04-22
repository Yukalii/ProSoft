using System;

namespace EasySave.Model.Logging
{
    public interface ILogger
    {
        void LogEntry(LogEntry entry);
    }
}
