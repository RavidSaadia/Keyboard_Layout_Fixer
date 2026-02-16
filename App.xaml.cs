using System.Windows;
using System.Drawing;
using System.Windows.Input;
using System.IO;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Clipboard = System.Windows.Clipboard;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using ContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using ToolStripMenuItem = System.Windows.Forms.ToolStripMenuItem;
using ToolStripSeparator = System.Windows.Forms.ToolStripSeparator;
using SendKeys = System.Windows.Forms.SendKeys;

namespace KeyboardLayoutFixer
{
    /// <summary>
    /// Main application class that manages the system tray icon and global hotkey
    /// </summary>
    public partial class App : Application
    {
        public const string AppVersion = "1.1.0";

        private static Mutex? _mutex;
        private NotifyIcon? _notifyIcon;
        private GlobalHotkey? _hotkey;
        private SettingsManager _settingsManager;
        private KeyboardLayoutConverter _converter;
        private MainWindow? _settingsWindow;

        public App()
        {
            _settingsManager = new SettingsManager();
            _converter = new KeyboardLayoutConverter();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                LogDebug("Application starting...");

                // Close any existing instances before starting
                CloseExistingInstances();
                _mutex = new Mutex(true, "KeyboardLayoutFixer_SingleInstance");

                // Load settings
                _settingsManager.LoadSettings();
                LogDebug("Settings loaded");

                // Create main window first (needed for hotkey registration)
                // but keep it hidden
                MainWindow = new MainWindow(_settingsManager);
                MainWindow.Hide();
                LogDebug("MainWindow created and hidden");

                // Create system tray icon
                CreateTrayIcon();
                LogDebug("Tray icon created");

                // Register global hotkey (needs MainWindow handle)
                RegisterHotkey();
                LogDebug("Hotkey registration attempted");
            }
            catch (Exception ex)
            {
                LogDebug($"FATAL ERROR in Application_Startup: {ex}");
                MessageBox.Show(
                    $"Application failed to start:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                    "Fatal Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Shutdown();
            }
        }

        private void LogDebug(string message)
        {
            try
            {
                var logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "KeyboardLayoutFixer",
                    "debug.log"
                );
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                File.AppendAllText(logPath, $"[{timestamp}] {message}\n");
                System.Diagnostics.Debug.WriteLine($"[{timestamp}] {message}");
            }
            catch { }
        }

        private void CreateTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = CreateDefaultIcon(),
                Visible = true,
                Text = $"Keyboard Layout Fixer v{AppVersion}\nHotkey: {_settingsManager.Settings.HotkeyDescription}"
            };

            // Create context menu
            var contextMenu = new ContextMenuStrip();

            var settingsItem = new ToolStripMenuItem("Settings");
            settingsItem.Click += (s, e) => ShowSettings();
            contextMenu.Items.Add(settingsItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => ExitApplication();
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => ShowSettings();

            // Show notification that app is running
            _notifyIcon.BalloonTipTitle = "Keyboard Layout Fixer";
            _notifyIcon.BalloonTipText = $"Running in background. Hotkey: {_settingsManager.Settings.HotkeyDescription}";
            _notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            _notifyIcon.ShowBalloonTip(3000);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);

        private System.Drawing.Icon CreateDefaultIcon()
        {
            // Create a simple icon programmatically using a bitmap
            using var bitmap = new System.Drawing.Bitmap(32, 32);
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                // Draw with design system gradient (red to orange)
                using (var gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new System.Drawing.Rectangle(0, 0, 32, 32),
                    System.Drawing.Color.FromArgb(235, 0, 44),   // #EB002C
                    System.Drawing.Color.FromArgb(255, 110, 30), // #FF6E1E
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                {
                    graphics.FillRectangle(gradientBrush, 0, 0, 32, 32);
                }

                // Draw "KB" text in white
                using (var font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold))
                using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
                {
                    graphics.DrawString("KB", font, brush, 3, 8);
                }
            }

            // Convert bitmap to icon, clone to own the handle, then clean up
            var iconHandle = bitmap.GetHicon();
            using var tempIcon = System.Drawing.Icon.FromHandle(iconHandle);
            var icon = (System.Drawing.Icon)tempIcon.Clone();
            DestroyIcon(iconHandle);
            return icon;
        }

        private void RegisterHotkey()
        {
            try
            {
                var settings = _settingsManager.Settings;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Attempting to register hotkey: {settings.HotkeyDescription}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Modifiers: {settings.HotkeyModifiers}, Key: {settings.HotkeyKey}");

                _hotkey = new GlobalHotkey(
                    settings.HotkeyModifiers,
                    settings.HotkeyKey,
                    OnHotkeyPressed
                );
                _hotkey.Register();

                System.Diagnostics.Debug.WriteLine("[DEBUG] Hotkey registered successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Hotkey registration failed: {ex}");
                MessageBox.Show(
                    $"Failed to register hotkey: {ex.Message}\n\nStack trace:\n{ex.StackTrace}\n\nPlease try changing the hotkey in settings.",
                    "Hotkey Registration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void UnregisterHotkey()
        {
            _hotkey?.Unregister();
            _hotkey?.Dispose();
            _hotkey = null;
        }

        /// <summary>
        /// Called when the global hotkey is pressed
        /// </summary>
        private void OnHotkeyPressed()
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] ===== HOTKEY PRESSED =====");

            try
            {
                // Store original clipboard content
                string originalClipboard = "";
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        originalClipboard = Clipboard.GetText();
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Original clipboard: {originalClipboard}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Error reading clipboard: {ex.Message}");
                }

                // Clear clipboard before Ctrl+C so we can reliably detect if text was selected
                Thread.Sleep(50);
                Clipboard.Clear();
                Thread.Sleep(50);

                // Send Ctrl+C to copy selected text (or nothing if no selection)
                SendKeys.SendWait("^c");
                Thread.Sleep(100);

                string capturedText = "";
                bool hasSelection = Clipboard.ContainsText();

                if (hasSelection)
                {
                    capturedText = Clipboard.GetText();
                }

                // If no selection, select all and copy
                if (!hasSelection)
                {
                    SendKeys.SendWait("^a");
                    Thread.Sleep(50);
                    SendKeys.SendWait("^c");
                    Thread.Sleep(100);

                    if (Clipboard.ContainsText())
                    {
                        capturedText = Clipboard.GetText();
                    }
                }

                // Check if text is empty
                if (string.IsNullOrEmpty(capturedText))
                {
                    RestoreClipboard(originalClipboard);
                    return;
                }

                // Check character limit
                var settings = _settingsManager.Settings;
                if (settings.MaxCharacterLimit > 0 && capturedText.Length > settings.MaxCharacterLimit)
                {
                    RestoreClipboard(originalClipboard);
                    return; // Abort operation silently
                }

                // Convert text based on settings
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Captured text: '{capturedText}'");
                string convertedText = _converter.Convert(capturedText, settings.MixedTextMode, settings.ReplaceCaps);
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Converted text: '{convertedText}'");

                // If text hasn't changed, don't replace
                if (convertedText == capturedText)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Text unchanged, aborting");
                    RestoreClipboard(originalClipboard);
                    return;
                }

                // Put converted text in clipboard
                Clipboard.SetText(convertedText);
                Thread.Sleep(50);

                // Paste the converted text
                SendKeys.SendWait("^v");
                Thread.Sleep(300);

                // Switch keyboard language if enabled
                if (settings.SwitchLanguageAfterConvert)
                {
                    Thread.Sleep(50);
                    SendKeys.SendWait("%+");
                }

                // Restore original clipboard content
                RestoreClipboard(originalClipboard);
            }
            catch (Exception ex)
            {
                // Silent fail - don't interrupt user's workflow
                System.Diagnostics.Debug.WriteLine($"Hotkey processing error: {ex.Message}");
            }
        }

        private void RestoreClipboard(string originalContent)
        {
            try
            {
                if (!string.IsNullOrEmpty(originalContent))
                {
                    Clipboard.SetText(originalContent);
                }
                else
                {
                    Clipboard.Clear();
                }
            }
            catch { }
        }

        private void ShowSettings()
        {
            if (_settingsWindow == null || !_settingsWindow.IsLoaded)
            {
                _settingsWindow = new MainWindow(_settingsManager);
                _settingsWindow.HotkeyChanged += OnHotkeyChanged;
            }

            _settingsWindow.Show();
            _settingsWindow.Activate();
        }

        private void OnHotkeyChanged(object? sender, EventArgs e)
        {
            // Unregister old hotkey
            UnregisterHotkey();

            // Register new hotkey
            RegisterHotkey();
        }

        private void ExitApplication()
        {
            _notifyIcon?.Dispose();
            Application.Current.Shutdown();
        }

        private static void CloseExistingInstances()
        {
            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            var existingProcesses = System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName);

            foreach (var process in existingProcesses)
            {
                if (process.Id != currentProcess.Id)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(3000);
                    }
                    catch { }
                }
                process.Dispose();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            UnregisterHotkey();
            _notifyIcon?.Dispose();
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
    }
}
