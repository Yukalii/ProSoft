using System;
using System.IO;
using System.Text.Json;

namespace EasySave.Model.Config
{
    /// <summary>
    /// Handles loading and saving application configuration.
    /// Stores user preferences such as language, log directory,
    /// status file path, and other global settings.
    /// </summary>
    public class ConfigManager
    {
        private readonly string _configFilePath;

        public AppConfig Config { get; private set; }

        public ConfigManager(string configFilePath)
        {
            _configFilePath = configFilePath;

            if (!File.Exists(_configFilePath))
            {
                Config = AppConfig.CreateDefault();
                Save();
            }
            else
            {
                Load();
            }
        }

        /// <summary>
        /// Loads configuration from the JSON file.
        /// </summary>
        public void Load()
        {
            string json = File.ReadAllText(_configFilePath);
            Config = JsonSerializer.Deserialize<AppConfig>(json)
                     ?? AppConfig.CreateDefault();
        }

        /// <summary>
        /// Saves the current configuration to the JSON file.
        /// </summary>
        public void Save()
        {
            string json = JsonSerializer.Serialize(Config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_configFilePath, json);
        }
    }
}
