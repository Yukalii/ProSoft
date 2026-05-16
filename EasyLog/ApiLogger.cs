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
                    // FIX : Tout le processus de préparation et d'envoi doit être DANS le bloc try
                    string json = JsonSerializer.Serialize(entry);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Envoi au serveur Docker
                    await _http.PostAsync($"{_serverUrl}/api/logs", content);
                }
                catch (Exception ex)
                {
                    // Écrit l'erreur réseau dans la fenêtre de Sortie de Visual Studio sans faire crasher l'application
                    System.Diagnostics.Debug.WriteLine($"[API LOGGER ERROR] Impossible d'envoyer le log au serveur : {ex.Message}");
                }
            });
        }
    }
}
