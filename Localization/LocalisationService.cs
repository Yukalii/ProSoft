using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Localisation
{
    /// <summary>
    /// Loads and provides access to localized strings.
    /// Language files are JSON dictionaries stored in a folder.
    /// </summary>
    public class LocalisationService
    {
        private readonly string _languagesDirectory;
        private Dictionary<string, string> _translations = new();

        public string CurrentLanguage { get; private set; } = "en";

        public LocalisationService(string languagesDirectory)
        {
            _languagesDirectory = languagesDirectory;

            if (!Directory.Exists(_languagesDirectory))
                Directory.CreateDirectory(_languagesDirectory);

            LoadLanguage("en"); // default 
        }

        /// <summary>
        /// Loads a language file ("en.json", "fr.json",...).
        /// </summary>
        public void LoadLanguage(string languageCode)
        {
            string filePath = Path.Combine(_languagesDirectory, $"{languageCode}.json");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Language file not found: {filePath}");

            string json = File.ReadAllText(filePath);

            _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                            ?? new Dictionary<string, string>();

            CurrentLanguage = languageCode;
        }

        /// <summary>
        /// Retrieves a translated string by key.
        /// Returns the key itself if no translation is found.
        /// </summary>
        public string Translate(string key)
        {
            if (_translations.TryGetValue(key, out string? value))
                return value;

            return key; // fallback
        }
    }
}
