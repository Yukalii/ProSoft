namespace EasySave.Localisation
{
    /// <summary>
    /// Centralized list of all localisation keys used in the application.
    /// </summary>
    public static class LanguageKeys
    {
        // General UI
        public const string AppTitle = "AppTitle";
        public const string Settings = "Settings";
        public const string Language = "Language";
        public const string Exit = "Exit";
        public const string Back = "Back";
        public const string Choice = "Choice";
        public const string InvalidChoice = "InvalidChoice";
        public const string SettingsSaved = "SettingsSaved";
        public const string MaxJobsReached = "MaxJobsReached";

        // Backup job management
        public const string ListJobs = "ListJobs";
        public const string CreateJob = "CreateJob";
        public const string EditJob = "EditJob";
        public const string DeleteJob = "DeleteJob";
        public const string AddJob = "AddJob";
        public const string RunJob = "RunJob";
        public const string JobName = "JobName";
        public const string Name = "Name";
        public const string SourcePath = "SourcePath";
        public const string TargetPath = "TargetPath";
        public const string BackupType = "BackupType";
        public const string Strategy = "Strategy";
        public const string JobCreated = "JobCreated";
        public const string JobDeleted = "JobDeleted";
        public const string JobNotFound = "JobNotFound";
        public const string EnterJobName = "EnterJobName";

        // Backup actions
        public const string StartBackup = "StartBackup";
        public const string StopBackup = "StopBackup";
        public const string BackupInProgress = "BackupInProgress";
        public const string BackupCompleted = "BackupCompleted";
        public const string BackupFinished = "BackupFinished";

        // Status / progress
        public const string Job = "Job";
        public const string State = "State";
        public const string Files = "Files";
        public const string Size = "Size";
        public const string Current = "Current";
        public const string TotalFiles = "TotalFiles";
        public const string ProcessedFiles = "ProcessedFiles";
        public const string TotalSize = "TotalSize";
        public const string ProcessedSize = "ProcessedSize";
        public const string CurrentFile = "CurrentFile";

        // Formats
        public const string LogFormat = "LogFormat";
        public const string LogFormatJson = "LogFormatJson";
        public const string LogFormatXml = "LogFormatXml";
        public const string LogFormatSaved = "LogFormatSaved";

        // Errors
        public const string Error = "Error";
        public const string InvalidPath = "InvalidPath";
        public const string JobAlreadyExists = "JobAlreadyExists";
    }
}
