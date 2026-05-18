using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace EasyLog
{
    /// <summary>
    /// Sends log entries to the centralized Docker log server via HTTP POST.
    /// </summary>
    public class ApiLogger : ILogger
    {
        private static readonly HttpClient _http = new();
        private readonly string _serverUrl;
        private readonly Func<string> _getFormat;
        private static readonly XmlSerializer XmlSerializer = new(typeof(LogEntry));

        public ApiLogger(string serverUrl, Func<string> getFormat)
        {
            _serverUrl = serverUrl.TrimEnd('/');
            _getFormat = getFormat;
        }

        public void LogEntry(LogEntry entry)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    string payload;
                    string contentType;

                    if (_getFormat().ToLower() == "xml")
                    {
                        XmlSerializerNamespaces namespaces = new();
                        namespaces.Add(string.Empty, string.Empty);
                        using StringWriter stringWriter = new();
                        XmlSerializer.Serialize(stringWriter, entry, namespaces);

                        payload = RemoveXmlDeclaration(stringWriter.ToString());
                        contentType = "application/xml";
                    }
                    else
                    {
                        payload = JsonSerializer.Serialize(entry);
                        contentType = "application/json";
                    }

                    var content = new StringContent(payload, Encoding.UTF8, contentType);
                    await _http.PostAsync($"{_serverUrl}/api/logs", content);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[API LOGGER ERROR] : {ex.Message}");
                }
            });
        }
        private static string RemoveXmlDeclaration(string xml)
        {
            int index = xml.IndexOf("?>", StringComparison.Ordinal);
            return index >= 0 ? xml[(index + 2)..].TrimStart() : xml;
        }
    }
}
