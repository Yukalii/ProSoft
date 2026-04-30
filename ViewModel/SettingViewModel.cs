using EasySave.Model.Config;
using EasySave.Localisation;
using EasySave.Model.Logger;

namespace EasySave.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ConfigManager _configManager;
        private readonly LocalisationService _localisation;
        private readonly DynamicLogger _dynamicLogger;

        public List<string> Languages { get; } = new List<string> { "en", "fr" };
        public List<string> LogFormats { get; } = new List<string> { "json", "xml" };

        public ObservableCollection<string> EncryptedExtensions { get; }

        public string SelectedLanguage
        {
            get => _configManager.Config.Language;
            set
            {
                _configManager.Config.Language = value;
                OnPropertyChanged();
            }
        }

        public string SelectedLogFormat
        {
            get => _configManager.Config.LogFormat;
            set
            {
                _configManager.Config.LogFormat = value;
                OnPropertyChanged();
            }
        }

        public string CryptoSoftKey
        {
            get => _configManager.Config.CryptoSoftKey;
            set
            {
                _configManager.Config.CryptoSoftKey = value;
                OnPropertyChanged();
            }
        }

        private string _newExtension = string.Empty;
        public string NewExtension
        {
            get => _newExtension;
            set
            {
                _newExtension = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand AddExtensionCommand { get; }
        public ICommand RemoveExtensionCommand { get; }

        public SettingsViewModel(
            LocalisationService localisation,
            ConfigManager configManager,
            DynamicLogger dynamicLogger)
        {
            _localisation = localisation;
            _configManager = configManager;
            _dynamicLogger = dynamicLogger;

            EncryptedExtensions = new ObservableCollection<string>(
                                       configManager.Config.EncryptedExtensions);

            SaveCommand = new RelayCommand(_ => SaveSettings());

            AddExtensionCommand = new RelayCommand(
                _ => AddExtension(),
                _ => !string.IsNullOrWhiteSpace(NewExtension));

            RemoveExtensionCommand = new RelayCommand(
                ext => RemoveExtension(ext as string));
        }

        private void AddExtension()
        {
            string ext = NewExtension.Trim();
            if (!ext.StartsWith('.')) ext = "." + ext;

            if (!EncryptedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                EncryptedExtensions.Add(ext);

            NewExtension = string.Empty;
        }

        private void RemoveExtension(string? ext)
        {
            if (ext is not null)
                EncryptedExtensions.Remove(ext);
        }

        public void SaveSettings()
        {
            _configManager.Config.EncryptedExtensions = EncryptedExtensions.ToList();
            _configManager.Save();

            _localisation.LoadLanguage(_configManager.Config.Language);

            ILogger newLogger = LoggerFactory.Resolve(
                _configManager.Config.LogFormat,
                _configManager.Config.LogDirectory);
            _dynamicLogger.SwapLogger(newLogger);
        }
    }
}