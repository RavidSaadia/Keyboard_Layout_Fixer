using System.Text;

namespace KeyboardLayoutFixer
{
    /// <summary>
    /// Converts text between English and Hebrew keyboard layouts
    /// </summary>
    public class KeyboardLayoutConverter
    {
        // Mapping from English characters to Hebrew characters (QWERTY to Hebrew keyboard)
        private readonly Dictionary<char, char> _englishToHebrew = new()
        {
            // Letters
            {'q', '/'}, {'Q', '/'},
            {'w', '\''}, {'W', '\''},
            {'e', 'ק'}, {'E', 'ק'},
            {'r', 'ר'}, {'R', 'ר'},
            {'t', 'א'}, {'T', 'א'},
            {'y', 'ט'}, {'Y', 'ט'},
            {'u', 'ו'}, {'U', 'ו'},
            {'i', 'ן'}, {'I', 'ן'},
            {'o', 'ם'}, {'O', 'ם'},
            {'p', 'פ'}, {'P', 'פ'},
            {'a', 'ש'}, {'A', 'ש'},
            {'s', 'ד'}, {'S', 'ד'},
            {'d', 'ג'}, {'D', 'ג'},
            {'f', 'כ'}, {'F', 'כ'},
            {'g', 'ע'}, {'G', 'ע'},
            {'h', 'י'}, {'H', 'י'},
            {'j', 'ח'}, {'J', 'ח'},
            {'k', 'ל'}, {'K', 'ל'},
            {'l', 'ך'}, {'L', 'ך'},
            {'z', 'ז'}, {'Z', 'ז'},
            {'x', 'ס'}, {'X', 'ס'},
            {'c', 'ב'}, {'C', 'ב'},
            {'v', 'ה'}, {'V', 'ה'},
            {'b', 'נ'}, {'B', 'נ'},
            {'n', 'מ'}, {'N', 'מ'},
            {'m', 'ץ'}, {'M', 'ץ'},

            // Punctuation and special characters
            {',', 'ת'},
            {'.', 'ץ'},
            {'/', '.'},
            {';', 'ף'},
            {'\'', ','},
            {'[', ']'},
            {']', '['},
            {'-', '-'},
            {'=', '='}
        };

        // Reverse mapping from Hebrew to English
        private readonly Dictionary<char, char> _hebrewToEnglish;

        public KeyboardLayoutConverter()
        {
            // Create reverse mapping - use GroupBy to handle duplicate Hebrew characters
            // Prefer lowercase English characters in the reverse mapping
            _hebrewToEnglish = _englishToHebrew
                .GroupBy(kvp => kvp.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group.First().Key // Take the first English char for each Hebrew char
                );
        }

        /// <summary>
        /// Converts text based on the specified mode
        /// </summary>
        /// <param name="text">Text to convert</param>
        /// <param name="mode">Conversion mode</param>
        /// <returns>Converted text</returns>
        public string Convert(string text, MixedTextMode mode)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var result = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                char converted = c;

                switch (mode)
                {
                    case MixedTextMode.ToggleAll:
                        // Try both directions
                        if (_englishToHebrew.TryGetValue(c, out char hebrewChar))
                        {
                            converted = hebrewChar;
                        }
                        else if (_hebrewToEnglish.TryGetValue(c, out char englishChar))
                        {
                            converted = englishChar;
                        }
                        break;

                    case MixedTextMode.EnglishToHebrewOnly:
                        // Only convert English to Hebrew, leave Hebrew as-is
                        if (_englishToHebrew.TryGetValue(c, out char hebChar))
                        {
                            converted = hebChar;
                        }
                        break;

                    case MixedTextMode.HebrewToEnglishOnly:
                        // Only convert Hebrew to English, leave English as-is
                        if (_hebrewToEnglish.TryGetValue(c, out char engChar))
                        {
                            converted = engChar;
                        }
                        break;
                }

                result.Append(converted);
            }

            return result.ToString();
        }

        /// <summary>
        /// Detects if text contains Hebrew characters
        /// </summary>
        public bool ContainsHebrew(string text)
        {
            return text.Any(c => c >= '\u0590' && c <= '\u05FF');
        }

        /// <summary>
        /// Detects if text contains English letters
        /// </summary>
        public bool ContainsEnglish(string text)
        {
            return text.Any(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'));
        }

        /// <summary>
        /// Detects if text contains a mix of both Hebrew and English
        /// </summary>
        public bool IsMixedText(string text)
        {
            return ContainsHebrew(text) && ContainsEnglish(text);
        }
    }

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
