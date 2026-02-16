using Newtonsoft.Json;

namespace KeyboardLayoutFixer.Models
{
    /// <summary>
    /// Application settings data class
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Modifier keys for the global hotkey (Ctrl, Alt, Shift, Win)
        /// </summary>
        public HotkeyModifiers HotkeyModifiers { get; set; } = HotkeyModifiers.Control;

        /// <summary>
        /// The key for the global hotkey
        /// </summary>
        public HotkeyKey HotkeyKey { get; set; } = HotkeyKey.Q;

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
        /// Whether to automatically switch keyboard language after conversion
        /// </summary>
        public bool SwitchLanguageAfterConvert { get; set; } = false;

        /// <summary>
        /// Validates settings and resets invalid values to defaults
        /// </summary>
        public void Validate()
        {
            if (HotkeyModifiers == HotkeyModifiers.None)
            {
                HotkeyModifiers = HotkeyModifiers.Control;
            }
        }

        /// <summary>
        /// Gets a human-readable description of the hotkey
        /// </summary>
        [JsonIgnore]
        public string HotkeyDescription
        {
            get
            {
                var parts = new List<string>();

                if (HotkeyModifiers.HasFlag(HotkeyModifiers.Control))
                    parts.Add("Ctrl");
                if (HotkeyModifiers.HasFlag(HotkeyModifiers.Alt))
                    parts.Add("Alt");
                if (HotkeyModifiers.HasFlag(HotkeyModifiers.Shift))
                    parts.Add("Shift");
                if (HotkeyModifiers.HasFlag(HotkeyModifiers.Win))
                    parts.Add(OperatingSystem.IsMacOS() ? "Cmd" : "Win");

                parts.Add(HotkeyKey.ToString());

                return string.Join("+", parts);
            }
        }
    }
}
