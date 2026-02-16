using KeyboardLayoutFixer.Models;

namespace KeyboardLayoutFixer.Platform
{
    /// <summary>
    /// Platform abstraction for OS-specific operations.
    /// Windows and macOS have different implementations.
    /// </summary>
    public interface IPlatformServices : IDisposable
    {
        /// <summary>
        /// Registers a global hotkey. Calls onPressed when the hotkey is triggered.
        /// The callback may be invoked on any thread; callers must dispatch to UI thread if needed.
        /// </summary>
        void RegisterHotkey(HotkeyModifiers modifiers, HotkeyKey key, Action onPressed);

        /// <summary>
        /// Unregisters the currently registered global hotkey.
        /// </summary>
        void UnregisterHotkey();

        /// <summary>
        /// Simulates Ctrl+C (Windows) or Cmd+C (macOS) to copy the current selection.
        /// </summary>
        Task SimulateCopyAsync();

        /// <summary>
        /// Simulates Ctrl+V (Windows) or Cmd+V (macOS) to paste from clipboard.
        /// </summary>
        Task SimulatePasteAsync();

        /// <summary>
        /// Simulates Ctrl+A (Windows) or Cmd+A (macOS) to select all text.
        /// </summary>
        Task SimulateSelectAllAsync();

        /// <summary>
        /// Simulates the OS keyboard language switch shortcut.
        /// Windows: Alt+Shift, macOS: platform-specific input source switching.
        /// </summary>
        Task SimulateLanguageSwitchAsync();

        /// <summary>
        /// Blocks until all modifier keys (Ctrl, Alt, Shift, Win/Cmd) are released,
        /// or until the timeout elapses.
        /// </summary>
        void WaitForModifierKeysRelease(int timeoutMs = 1000);

        /// <summary>
        /// Gets the current text from the OS clipboard. Returns null if empty or not text.
        /// Must be called from the UI (STA) thread.
        /// </summary>
        string? GetClipboardText();

        /// <summary>
        /// Sets text on the OS clipboard.
        /// Must be called from the UI (STA) thread.
        /// </summary>
        void SetClipboardText(string text);

        /// <summary>
        /// Clears the OS clipboard.
        /// Must be called from the UI (STA) thread.
        /// </summary>
        void ClearClipboard();
    }
}
