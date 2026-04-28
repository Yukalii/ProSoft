using System;

namespace EasySave.Model.Logger
{
    public interface ILogger
    {
        void LogEntry(LogEntry entry);
    }
}
