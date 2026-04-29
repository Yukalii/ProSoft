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

        public ICommand SaveCommand { get; }

        public SettingsViewModel(
            LocalisationService localisation,
            ConfigManager configManager,
            DynamicLogger dynamicLogger)
        {
            _localisation = localisation;
            _configManager = configManager;
            _dynamicLogger = dynamicLogger;

            SaveCommand = new RelayCommand(_ => SaveSettings());
        }

        public void SaveSettings()
        {
            _configManager.Save();
            _localisation.LoadLanguage(_configManager.Config.Language);

            ILogger newLogger = LoggerFactory.Resolve(
                _configManager.Config.LogFormat,
                _configManager.Config.LogDirectory
            );
            _dynamicLogger.SwapLogger(newLogger);
        }
    }
}