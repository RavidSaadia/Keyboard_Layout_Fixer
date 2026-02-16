namespace KeyboardLayoutFixer.Models
{
    /// <summary>
    /// Platform-agnostic key enum for hotkey configuration.
    /// Integer values match System.Windows.Input.Key to maintain backward
    /// compatibility with existing settings.json files.
    /// </summary>
    public enum HotkeyKey
    {
        None = 0,
        Tab = 3,
        Enter = 6,
        Space = 18,
        A = 44,
        B = 45,
        C = 46,
        D = 47,
        E = 48,
        F = 49,
        G = 50,
        H = 51,
        I = 52,
        J = 53,
        K = 54,
        L = 55,
        M = 56,
        N = 57,
        O = 58,
        P = 59,
        Q = 60,
        R = 61,
        S = 62,
        T = 63,
        U = 64,
        V = 65,
        W = 66,
        X = 67,
        Y = 68,
        Z = 69,
        F1 = 90,
        F2 = 91,
        F3 = 92,
        F4 = 93,
        F5 = 94,
        F6 = 95,
        F7 = 96,
        F8 = 97,
        F9 = 98,
        F10 = 99,
        F11 = 100,
        F12 = 101
    }

    public static class HotkeyKeyExtensions
    {
        /// <summary>
        /// Maps HotkeyKey to Win32 virtual key code for RegisterHotKey.
        /// </summary>
        public static uint ToVirtualKeyCode(this HotkeyKey key)
        {
            return key switch
            {
                HotkeyKey.Tab => 0x09,
                HotkeyKey.Enter => 0x0D,
                HotkeyKey.Space => 0x20,
                >= HotkeyKey.A and <= HotkeyKey.Z => (uint)(0x41 + (key - HotkeyKey.A)),
                >= HotkeyKey.F1 and <= HotkeyKey.F12 => (uint)(0x70 + (key - HotkeyKey.F1)),
                _ => 0
            };
        }
    }
}
