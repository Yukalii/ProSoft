namespace EasyLog
{
    /// <summary>
    /// Defines where log entries are written.
    /// Exposed as a user setting in the ProSoft Settings view.
    /// </summary>
    public enum LogStorageMode
    {
        /// <summary>Logs are written only to the local machine.</summary>
        LocalOnly,

        /// <summary>Logs are sent only to the centralized Docker server.</summary>
        CentralizedOnly,

        /// <summary>Logs are written both locally and to the Docker server.</summary>
        Both
    }
}
