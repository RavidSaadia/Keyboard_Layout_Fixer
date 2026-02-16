using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using KeyboardLayoutFixer.Models;
using KeyboardLayoutFixer.Platform;
using KeyboardLayoutFixer.Services;
using KeyboardLayoutFixer.Views;
using System.IO;

namespace KeyboardLayoutFixer
{
    public partial class App : Application
    {
        public const string AppVersion = "1.2.7";

        private static Mutex? _mutex;
        private IPlatformServices? _platformServices;
        private SettingsManager _settingsManager = new();
        private KeyboardLayoutConverter _converter = new();
        private MainWindow? _settingsWindow;
        private bool _isEnabled = true;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                try
                {
                    LogDebug("Application starting...");

                    // Single instance check
                    CloseExistingInstances();
                    _mutex = new Mutex(true, "KeyboardLayoutFixer_SingleInstance");

                    // Load settings
                    _settingsManager.LoadSettings();
                    LogDebug("Settings loaded");

                    // Create platform services
                    _platformServices = PlatformServiceFactory.Create();
                    LogDebug("Platform services created");

                    // Register global hotkey
                    RegisterHotkey();
                    LogDebug("Hotkey registration attempted");

                    // Update tray icon tooltip with version
                    var trayIcons = TrayIcon.GetIcons(this);
                    if (trayIcons?.Count > 0)
                        trayIcons[0].ToolTipText = $"Keyboard Layout Fixer v{AppVersion}";

                    // Tray-only app: no main window, shutdown only on explicit request
                    desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                    desktop.ShutdownRequested += (_, _) =>
                    {
                        _platformServices?.UnregisterHotkey();
                        _platformServices?.Dispose();
                        _mutex?.ReleaseMutex();
                        _mutex?.Dispose();
                    };
                }
                catch (Exception ex)
                {
                    LogDebug($"FATAL ERROR in startup: {ex}");
                    System.Diagnostics.Debug.WriteLine($"FATAL: {ex}");
                    desktop.Shutdown(1);
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void RegisterHotkey()
        {
            try
            {
                var settings = _settingsManager.Settings;
                _platformServices!.RegisterHotkey(
                    settings.HotkeyModifiers,
                    settings.HotkeyKey,
                    () => Dispatcher.UIThread.Post(OnHotkeyPressed)
                );
                LogDebug($"Hotkey registered: {settings.HotkeyDescription}");
            }
            catch (Exception ex)
            {
                LogDebug($"Hotkey registration failed: {ex}");
                System.Diagnostics.Debug.WriteLine($"Hotkey registration failed: {ex.Message}");
            }
        }

        private void UnregisterHotkey()
        {
            _platformServices?.UnregisterHotkey();
        }

        private async void OnHotkeyPressed()
        {
            if (!_isEnabled)
            {
                LogDebug("Hotkey pressed but app is disabled, ignoring");
                return;
            }

            LogDebug("===== HOTKEY PRESSED =====");

            try
            {
                // Wait for modifier keys to release
                await Task.Run(() => _platformServices!.WaitForModifierKeysRelease());
                LogDebug("Modifier keys released");

                // Store original clipboard content
                string? originalClipboard = null;
                try
                {
                    originalClipboard = _platformServices!.GetClipboardText();
                    LogDebug($"Original clipboard: {originalClipboard}");
                }
                catch (Exception ex)
                {
                    LogDebug($"Error reading clipboard: {ex.Message}");
                }

                // Clear clipboard before copy
                _platformServices!.ClearClipboard();
                await Task.Delay(50);

                // Simulate Ctrl+C / Cmd+C
                await _platformServices.SimulateCopyAsync();

                // Read captured text
                string? capturedText = _platformServices.GetClipboardText();
                bool hasSelection = !string.IsNullOrEmpty(capturedText);
                LogDebug($"After Ctrl+C, captured: '{capturedText}', hasSelection: {hasSelection}");

                // If no selection, select all and copy
                if (!hasSelection)
                {
                    await _platformServices.SimulateSelectAllAsync();
                    await _platformServices.SimulateCopyAsync();
                    capturedText = _platformServices.GetClipboardText();
                    LogDebug($"After Select All + Ctrl+C, captured: '{capturedText}'");
                }

                if (string.IsNullOrEmpty(capturedText))
                {
                    LogDebug("No text captured, restoring clipboard");
                    RestoreClipboard(originalClipboard);
                    return;
                }

                // Check character limit
                var settings = _settingsManager.Settings;
                if (settings.MaxCharacterLimit > 0 && capturedText.Length > settings.MaxCharacterLimit)
                {
                    LogDebug($"Text exceeds limit ({capturedText.Length} > {settings.MaxCharacterLimit})");
                    RestoreClipboard(originalClipboard);
                    return;
                }

                // Convert text
                LogDebug($"Captured text: '{capturedText}'");
                string convertedText = _converter.Convert(capturedText, settings.MixedTextMode, settings.ReplaceCaps);
                LogDebug($"Converted text: '{convertedText}'");

                if (convertedText == capturedText)
                {
                    LogDebug("Text unchanged, aborting");
                    RestoreClipboard(originalClipboard);
                    return;
                }

                // Put converted text in clipboard and paste
                _platformServices.SetClipboardText(convertedText);
                await Task.Delay(50);

                await _platformServices.SimulatePasteAsync();
                LogDebug("Paste simulated");

                // Switch keyboard language if enabled
                if (settings.SwitchLanguageAfterConvert)
                {
                    await Task.Delay(50);
                    await _platformServices.SimulateLanguageSwitchAsync();
                    LogDebug("Language switch simulated");
                }

                // Restore original clipboard
                await Task.Delay(200);
                RestoreClipboard(originalClipboard);
                LogDebug("Clipboard restored, done");
            }
            catch (Exception ex)
            {
                LogDebug($"Hotkey processing error: {ex}");
            }
        }

        private void RestoreClipboard(string? originalContent)
        {
            try
            {
                if (!string.IsNullOrEmpty(originalContent))
                    _platformServices!.SetClipboardText(originalContent);
                else
                    _platformServices!.ClearClipboard();
            }
            catch { }
        }

        // --- Tray Icon Event Handlers ---

        private void TrayIcon_Clicked(object? sender, EventArgs e)
        {
            ShowSettings();
        }

        private void Settings_Click(object? sender, EventArgs e)
        {
            ShowSettings();
        }

        private void Toggle_Click(object? sender, EventArgs e)
        {
            _isEnabled = !_isEnabled;
            if (sender is NativeMenuItem menuItem)
            {
                menuItem.Header = _isEnabled ? "Disable" : "Enable";
            }
            LogDebug($"App {(_isEnabled ? "enabled" : "disabled")}");
        }

        private void Restart_Click(object? sender, EventArgs e)
        {
            RestartApplication();
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            ExitApplication();
        }

        private void ShowSettings()
        {
            if (_settingsWindow == null || !_settingsWindow.IsVisible)
            {
                _settingsWindow = new MainWindow(_settingsManager);
                _settingsWindow.HotkeyChanged += OnHotkeyChanged;
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.Activate();
            }
        }

        private void OnHotkeyChanged(object? sender, EventArgs e)
        {
            UnregisterHotkey();
            RegisterHotkey();
        }

        private void RestartApplication()
        {
            var exePath = Environment.ProcessPath;
            if (exePath != null)
            {
                System.Diagnostics.Process.Start(exePath);
            }
            ExitApplication();
        }

        private void ExitApplication()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
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

        private void LogDebug(string message)
        {
            try
            {
                var logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "KeyboardLayoutFixer",
                    "debug.log"
                );
                var dir = Path.GetDirectoryName(logPath);
                if (dir != null) Directory.CreateDirectory(dir);

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                File.AppendAllText(logPath, $"[{timestamp}] {message}\n");
                System.Diagnostics.Debug.WriteLine($"[{timestamp}] {message}");
            }
            catch { }
        }
    }
}
