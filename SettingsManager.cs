using Newtonsoft.Json;
using System.IO;
using System.Windows.Input;

namespace KeyboardLayoutFixer
{
    /// <summary>
    /// Manages application settings and persistence
    /// </summary>
    public class SettingsManager
    {
        private const string SETTINGS_FILENAME = "settings.json";
        private readonly string _settingsPath;

        public AppSettings Settings { get; private set; }

        public SettingsManager()
        {
            // Store settings in AppData folder
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
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    var loaded = JsonConvert.DeserializeObject<AppSettings>(json);
                    if (loaded != null)
                    {
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
            SaveSettings();
        }

        /// <summary>
        /// Saves current settings to disk
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                File.WriteAllText(_settingsPath, json);
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

    /// <summary>
    /// Application settings data class
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Modifier keys for the global hotkey (Ctrl, Alt, Shift, Win)
        /// </summary>
        public ModifierKeys HotkeyModifiers { get; set; } = ModifierKeys.Control;

        /// <summary>
        /// The key for the global hotkey
        /// </summary>
        public Key HotkeyKey { get; set; } = Key.Q;

        /// <summary>
        /// Maximum number of characters to process (0 = unlimited)
        /// </summary>
        public int MaxCharacterLimit { get; set; } = 0;

        /// <summary>
        /// How to handle text that contains both Hebrew and English
        /// </summary>
        public MixedTextMode MixedTextMode { get; set; } = MixedTextMode.ToggleAll;

        /// <summary>
        /// Whether to convert uppercase (CAPS) letters or leave them as-is
        /// </summary>
        public bool ReplaceCaps { get; set; } = true;

        /// <summary>
        /// Whether to automatically switch keyboard language (Alt+Shift) after conversion
        /// </summary>
        public bool SwitchLanguageAfterConvert { get; set; } = false;

        /// <summary>
        /// Gets a human-readable description of the hotkey
        /// </summary>
        [JsonIgnore]
        public string HotkeyDescription
        {
            get
            {
                var parts = new List<string>();

                if (HotkeyModifiers.HasFlag(ModifierKeys.Control))
                    parts.Add("Ctrl");
                if (HotkeyModifiers.HasFlag(ModifierKeys.Alt))
                    parts.Add("Alt");
                if (HotkeyModifiers.HasFlag(ModifierKeys.Shift))
                    parts.Add("Shift");
                if (HotkeyModifiers.HasFlag(ModifierKeys.Win))
                    parts.Add("Win");

                parts.Add(HotkeyKey.ToString());

                return string.Join("+", parts);
            }
        }
    }
}
