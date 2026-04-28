using System.Xml.Serialization;

namespace EasySave.Model.Logging
{
    /// <summary>
    /// Writes log entries to a daily XML file.
    /// Each entry is appended as an XML element inside a root LogEntries node.
    /// </summary>
    public class XmlLogger : ILogger
    {
        private readonly string _logDirectory;
        private static readonly XmlSerializer Serializer = new(typeof(LogEntry));

        public XmlLogger(string logDirectory)
        {
            _logDirectory = logDirectory;

            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
        }

        public void LogEntry(LogEntry entry)
        {
            string fileName = $"log_{DateTime.Now:yyyy-MM-dd}.xml";
            string filePath = Path.Combine(_logDirectory, fileName);

            // Build XML entry string without declaration
            XmlSerializerNamespaces namespaces = new();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter stringWriter = new();
            Serializer.Serialize(stringWriter, entry, namespaces);

            // Strip the XML declaration line to allow clean appending
            string xmlEntry = RemoveXmlDeclaration(stringWriter.ToString());

            File.AppendAllText(filePath, xmlEntry + Environment.NewLine);
        }

        private static string RemoveXmlDeclaration(string xml)
        {
            int index = xml.IndexOf("?>", StringComparison.Ordinal);
            return index >= 0 ? xml[(index + 2)..].TrimStart() : xml;
        }
    }
}