using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace EasyLog
{
    /// <summary>
    /// Sends log entries to the centralized Docker log server via HTTP POST.
    /// </summary>
    public class ApiLogger : ILogger
    {
        private static readonly HttpClient _http = new();
        private readonly string _serverUrl;

        public ApiLogger(string serverUrl)
        {
            _serverUrl = serverUrl.TrimEnd('/');
        }

        public void LogEntry(LogEntry entry)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    string json = JsonSerializer.Serialize(entry);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await _http.PostAsync($"{_serverUrl}/api/logs", content);
                }
                catch
                {
                    // Network errors are silently ignored to avoid crashing the backup
                }
            });
        }
    }
}
