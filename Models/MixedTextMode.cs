namespace KeyboardLayoutFixer.Models
{
    /// <summary>
    /// Modes for handling text that contains both Hebrew and English characters
    /// </summary>
    public enum MixedTextMode
    {
        /// <summary>
        /// Toggle both English to Hebrew AND Hebrew to English
        /// </summary>
        ToggleAll,

        /// <summary>
        /// Convert only English to Hebrew, leave Hebrew characters unchanged
        /// </summary>
        EnglishToHebrewOnly,

        /// <summary>
        /// Convert only Hebrew to English, leave English characters unchanged
        /// </summary>
        HebrewToEnglishOnly
    }
}
