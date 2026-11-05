using Microsoft.Win32;

namespace KeyboardLayoutFixer.Services
{
    /// <summary>
    /// Manages the "Run at Windows startup" registry entry (HKCU\...\Run).
    /// </summary>
    public static class StartupManager
    {
        private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "KeyboardLayoutFixer";

        /// <summary>
        /// Adds or removes the startup entry. When enabling, the entry always points
        /// at the currently running exe, so moving the exe self-heals on next launch.
        /// </summary>
        public static void Apply(bool enable)
        {
            if (!OperatingSystem.IsWindows()) return;

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
                if (key == null) return;

                if (enable)
                {
                    var exePath = Environment.ProcessPath;
                    if (exePath != null)
                        key.SetValue(AppName, $"\"{exePath}\"");
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating startup entry: {ex.Message}");
            }
        }
    }
}
