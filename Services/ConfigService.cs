using SharedConfigApp.Models;
using System.Xml.Linq;

namespace SharedConfigApp.Services
{
    public class ConfigService : IDisposable
    {
        private readonly string _configPath;
        private readonly FileSystemWatcher _fileWatcher;
        private readonly ILogger<ConfigService> _logger;
        private ConfigModel _currentConfig;
        private readonly object _lock = new object();

        public ConfigService(IConfiguration configuration, ILogger<ConfigService> logger)
        {
            _logger = logger;
            _configPath = configuration["SharedConfigPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "SharedData", "config.xml");
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_configPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create initial config if it doesn't exist
            if (!File.Exists(_configPath))
            {
                CreateInitialConfig();
            }

            _currentConfig = LoadConfig();

            // Set up file watcher for automatic reload
            _fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(_configPath)!, Path.GetFileName(_configPath));
            _fileWatcher.Changed += OnConfigFileChanged;
            _fileWatcher.EnableRaisingEvents = true;

            _logger.LogInformation("ConfigService initialized with path: {ConfigPath}", _configPath);
        }

        public ConfigModel GetCurrentConfig()
        {
            lock (_lock)
            {
                // Always read fresh from file to get latest changes
                _currentConfig = LoadConfig();
                return _currentConfig;
            }
        }

        public bool UpdateConfig(ConfigModel config)
        {
            try
            {
                lock (_lock)
                {
                    SaveConfig(config);
                    _currentConfig = config;
                    _currentConfig.LastUpdated = DateTime.Now;
                }

                _logger.LogInformation("Configuration updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration");
                return false;
            }
        }

        private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            // Debounce file changes (file might be written in multiple operations)
            Task.Delay(100).ContinueWith(_ =>
            {
                try
                {
                    lock (_lock)
                    {
                        _currentConfig = LoadConfig();
                        _currentConfig.LastUpdated = DateTime.Now;
                    }

                    _logger.LogInformation("Configuration reloaded from file");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reloading configuration from file");
                }
            });
        }

        private ConfigModel LoadConfig()
        {
            try
            {
                var doc = XDocument.Load(_configPath);
                var appSettings = doc.Root?.Element("appSettings");

                if (appSettings == null)
                {
                    _logger.LogWarning("No appSettings section found in config file");
                    return new ConfigModel();
                }

                var config = new ConfigModel
                {
                    AppName = GetConfigValue(appSettings, "AppName") ?? "Default App",
                    DatabaseConnection = GetConfigValue(appSettings, "DatabaseConnection") ?? "Server=localhost;Database=DefaultDB;",
                    ApiTimeout = int.TryParse(GetConfigValue(appSettings, "ApiTimeout"), out var timeout) ? timeout : 30,
                    EnableLogging = bool.TryParse(GetConfigValue(appSettings, "EnableLogging"), out var logging) && logging,
                    MaxUsers = int.TryParse(GetConfigValue(appSettings, "MaxUsers"), out var maxUsers) ? maxUsers : 100,
                    LastUpdated = File.GetLastWriteTime(_configPath)
                };

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration from {ConfigPath}", _configPath);
                return new ConfigModel();
            }
        }

        private void SaveConfig(ConfigModel config)
        {
            var doc = new XDocument(
                new XElement("configuration",
                    new XElement("appSettings",
                        new XElement("add", new XAttribute("key", "AppName"), new XAttribute("value", config.AppName)),
                        new XElement("add", new XAttribute("key", "DatabaseConnection"), new XAttribute("value", config.DatabaseConnection)),
                        new XElement("add", new XAttribute("key", "ApiTimeout"), new XAttribute("value", config.ApiTimeout.ToString())),
                        new XElement("add", new XAttribute("key", "EnableLogging"), new XAttribute("value", config.EnableLogging.ToString().ToLower())),
                        new XElement("add", new XAttribute("key", "MaxUsers"), new XAttribute("value", config.MaxUsers.ToString()))
                    )
                )
            );

            doc.Save(_configPath);
        }

        private void CreateInitialConfig()
        {
            var initialConfig = new ConfigModel
            {
                AppName = "Shared Config Demo",
                DatabaseConnection = "Server=localhost;Database=MyApp;",
                ApiTimeout = 30,
                EnableLogging = true,
                MaxUsers = 100
            };

            SaveConfig(initialConfig);
            _logger.LogInformation("Created initial configuration file at {ConfigPath}", _configPath);
        }

        private string? GetConfigValue(XElement appSettings, string key)
        {
            return appSettings.Elements("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == key)
                ?.Attribute("value")?.Value;
        }

        public void Dispose()
        {
            _fileWatcher?.Dispose();
        }
    }
}