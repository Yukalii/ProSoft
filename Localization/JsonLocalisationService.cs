using System.Text.Json;

namespace EasySave.Localisation
{
    public class JsonLocalisationService : ILocalisationService
    {
        private readonly Dictionary<string, string> _translations;

        public JsonLocalisationService(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("localisation file not found.", filePath);

            string json = File.ReadAllText(filePath);
            _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                            ?? new Dictionary<string, string>();
        }

        public string GetString(string key)
        {
            return _translations.TryGetValue(key, out var value)
                ? value
                : $"[{key}]";
        }
    }
}
