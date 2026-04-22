using System;
using System.Collections.Generic;
using EasySave.Localization;
using EasySave.Model.Config;

namespace EasySave.ViewModel
{
    public class SettingsViewModel
    {
        private readonly LocalizationService _localization;
        private readonly ConfigManager _config;

        public List<string> AvailableLanguages { get; } =
            new List<string> { "en", "fr" };

        public string SelectedLanguage { get; set; }

        public SettingsViewModel(LocalizationService localization, ConfigManager config)
        {
            _localization = localization;
            _config = config;

            SelectedLanguage = _config.Config.Language;
        }

        public void SaveSettings()
        {
            _localization.LoadLanguage(SelectedLanguage);
            _config.Config.Language = SelectedLanguage;
            _config.Save();
        }

        public void Cancel()
        {
            // Nothing special for console
        }
    }
}
