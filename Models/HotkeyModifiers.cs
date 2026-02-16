namespace KeyboardLayoutFixer.Models
{
    /// <summary>
    /// Platform-agnostic modifier keys for hotkey combinations.
    /// Values match the Win32 RegisterHotKey constants and the original custom enum
    /// to maintain backward compatibility with existing settings.json files.
    /// </summary>
    [Flags]
    public enum HotkeyModifiers : uint
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
