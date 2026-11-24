using System.IO;
using System.Text.Json;
using KeyboardLayoutFixer.Models;

namespace KeyboardLayoutFixer.Services
{
    /// <summary>
    /// Manages application settings and persistence
    /// </summary>
    public class SettingsManager
    {
        private const string SETTINGS_FILENAME = "settings.json";
        private readonly string _settingsPath;
        private readonly object _lock = new object();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public AppSettings Settings { get; private set; }
        public bool IsFirstRun { get; private set; }

        public SettingsManager()
        {
            // Store settings in AppData folder (cross-platform)
            // Windows: %APPDATA%\KeyboardLayoutFixer\
            // macOS: ~/Library/Application Support/KeyboardLayoutFixer/
            // Linux: ~/.config/KeyboardLayoutFixer/
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "KeyboardLayoutFixer");
            Directory.CreateDirectory(appFolder);
            _settingsPath = Path.Combine(appFolder, SETTINGS_FILENAME);

            // Initialize with defaults
            Settings = new AppSettings();
        }

        /// <summary>
        /// Loads settings from disk, or creates default settings if file doesn't exist
        /// </summary>
        public void LoadSettings()
        {
            lock (_lock)
            {
                try
                {
                    IsFirstRun = !File.Exists(_settingsPath);
                    if (!IsFirstRun)
                    {
                        string json = File.ReadAllText(_settingsPath);
                        var loaded = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
                        if (loaded != null)
                        {
                            loaded.Validate();
                            Settings = loaded;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                }

                // If loading failed or file doesn't exist, use defaults
                Settings = new AppSettings();
                SaveSettingsInternal();
            }
        }

        /// <summary>
        /// Saves current settings to disk
        /// </summary>
        public void SaveSettings()
        {
            lock (_lock)
            {
                SaveSettingsInternal();
            }
        }

        private void SaveSettingsInternal()
        {
            try
            {
                string json = JsonSerializer.Serialize(Settings, _jsonOptions);

                // Write to a temp file first, then swap it in, so a crash or power
                // loss mid-write can never leave a half-written settings.json behind.
                string tempPath = _settingsPath + ".tmp";
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, _settingsPath, overwrite: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the settings file path
        /// </summary>
        public string GetSettingsPath() => _settingsPath;
    }
}
