using EasySave.Model.Config;
using EasySave.Localisation;

namespace EasySave.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ConfigManager _configManager;
        private readonly LocalisationService _localisation;
        private readonly DynamicLogger _dynamicLogger;
        private readonly int _largeFileThresholdKb;
        
        public List<string> Languages { get; } = new List<string> { "en", "fr" };
        public List<string> LogFormats { get; } = new List<string> { "json", "xml" };

        public ObservableCollection<string> EncryptedExtensions { get; }

        public List<string> LogStorageModes { get; } = Enum.GetNames<LogStorageMode>().ToList();

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

        public int LargeFileThresholdKb
        {
            get => _configManager.Config.LargeFileThresholdKb;
            set
            {
                _configManager.Config.LargeFileThresholdKb = value;
                OnPropertyChanged(); ; 
            }
        }

        public string BusinessApp
        {
            get => _configManager.Config.BusinessApp;
            set
            {
                _configManager.Config.BusinessApp = value;
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

        public string SelectedLogStorageMode
        {
            get => _configManager.Config.LogStorageMode.ToString();
            set
            {
                if (Enum.TryParse<LogStorageMode>(value, out var mode))
                {
                    _configManager.Config.LogStorageMode = mode;
                    OnPropertyChanged();
                }
            }
        }

        public string LogServerUrl
        {
            get => _configManager.Config.LogServerUrl;
            set { _configManager.Config.LogServerUrl = value; OnPropertyChanged(); }
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
            _largeFileThresholdKb = configManager.Config.LargeFileThresholdKb;

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
            _configManager.Config.LargeFileThresholdKb = LargeFileThresholdKb;
            _configManager.Save();

            _localisation.LoadLanguage(_configManager.Config.Language);

            ILogger newLogger = LoggerFactory.ResolveComposite(
                _configManager.Config.LogFormat,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configManager.Config.LogDirectory),
            _configManager.Config.LogServerUrl,
            () => _configManager.Config.LogStorageMode);
        }
    }
}