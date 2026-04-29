namespace EasySave.Localisation
{
    public class LocalisationService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly string _languagesDirectory;
        private Dictionary<string, string> _translations = new();

        public string CurrentLanguage { get; private set; } = "en";

        public string this[string key] => Translate(key);

        public LocalisationService(string languagesDirectory)
        {
            _languagesDirectory = languagesDirectory;

            if (!Directory.Exists(_languagesDirectory))
                Directory.CreateDirectory(_languagesDirectory);

            LoadLanguage("en");
        }

        public void LoadLanguage(string languageCode)
        {
            string filePath = Path.Combine(_languagesDirectory, $"{languageCode}.json");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Language file not found: {filePath}");

            string json = File.ReadAllText(filePath);
            _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            CurrentLanguage = languageCode;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Binding.IndexerName));
        }

        public string Translate(string key)
        {
            if (_translations.TryGetValue(key, out string? value))
                return value;
            return key;
        }
    }
}