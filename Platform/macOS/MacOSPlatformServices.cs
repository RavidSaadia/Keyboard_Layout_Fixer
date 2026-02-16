using KeyboardLayoutFixer.Models;

namespace KeyboardLayoutFixer.Platform.MacOS
{
    /// <summary>
    /// macOS implementation of platform services.
    /// Currently stubbed - requires Mac access for testing and implementation.
    ///
    /// When implemented, this will use:
    /// - SharpHook (IGlobalHook) for global keyboard hooks
    /// - CoreGraphics CGEvent APIs for keystroke simulation
    /// - CGEventSourceFlagsState for modifier key state polling
    /// </summary>
    public class MacOSPlatformServices : IPlatformServices
    {
        public void RegisterHotkey(HotkeyModifiers modifiers, HotkeyKey key, Action onPressed)
        {
            // TODO: Implement using SharpHook IGlobalHook
            // SharpHook fires on a background thread; dispatch to UI thread via
            // Avalonia.Threading.Dispatcher.UIThread.Post(onPressed)
            throw new NotImplementedException(
                "macOS global hotkey registration is not yet implemented. " +
                "Will use SharpHook library for cross-platform keyboard hooks.");
        }

        public void UnregisterHotkey()
        {
            // TODO: Dispose SharpHook hook
            throw new NotImplementedException(
                "macOS hotkey unregistration is not yet implemented.");
        }

        public Task SimulateCopyAsync()
        {
            // TODO: Use CGEventCreateKeyboardEvent + CGEventPost
            // Cmd+C: kVK_Command (0x37) + kVK_ANSI_C (0x08)
            throw new NotImplementedException(
                "macOS keystroke simulation (Cmd+C) is not yet implemented. " +
                "Will use CoreGraphics CGEvent APIs via P/Invoke.");
        }

        public Task SimulatePasteAsync()
        {
            // TODO: Cmd+V: kVK_Command (0x37) + kVK_ANSI_V (0x09)
            throw new NotImplementedException(
                "macOS keystroke simulation (Cmd+V) is not yet implemented.");
        }

        public Task SimulateSelectAllAsync()
        {
            // TODO: Cmd+A: kVK_Command (0x37) + kVK_ANSI_A (0x00)
            throw new NotImplementedException(
                "macOS keystroke simulation (Cmd+A) is not yet implemented.");
        }

        public Task SimulateLanguageSwitchAsync()
        {
            // TODO: macOS input source switching
            // Options: TISSelectInputSource via Carbon API, or Ctrl+Space shortcut
            throw new NotImplementedException(
                "macOS language switching is not yet implemented. " +
                "Will use Carbon TISSelectInputSource or system shortcut.");
        }

        public void WaitForModifierKeysRelease(int timeoutMs = 1000)
        {
            // TODO: Use CGEventSourceFlagsState(kCGEventSourceStateCombinedSessionState)
            // Poll for kCGEventFlagMaskShift, kCGEventFlagMaskControl,
            // kCGEventFlagMaskAlternate, kCGEventFlagMaskCommand
            throw new NotImplementedException(
                "macOS modifier key polling is not yet implemented. " +
                "Will use CGEventSourceFlagsState via CoreGraphics P/Invoke.");
        }

        public string? GetClipboardText()
        {
            throw new NotImplementedException(
                "macOS clipboard access is not yet implemented. " +
                "Will use NSPasteboard via ObjC runtime P/Invoke.");
        }

        public void SetClipboardText(string text)
        {
            throw new NotImplementedException(
                "macOS clipboard access is not yet implemented.");
        }

        public void ClearClipboard()
        {
            throw new NotImplementedException(
                "macOS clipboard access is not yet implemented.");
        }

        public void Dispose()
        {
            // No resources to clean up yet
        }
    }
}
