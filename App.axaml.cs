using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using KeyboardLayoutFixer.Models;
using KeyboardLayoutFixer.Platform;
using KeyboardLayoutFixer.Services;
using KeyboardLayoutFixer.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.IO;
using Icon = MsBox.Avalonia.Enums.Icon;

namespace KeyboardLayoutFixer
{
    public partial class App : Application
    {
        /// <summary>
        /// App version (Major.Minor.Build), taken from &lt;Version&gt; in the csproj / the CI tag.
        /// </summary>
        public static readonly string AppVersion = GetAppVersion();

        private static readonly TimeSpan FirstUpdateCheckDelay = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan UpdateCheckInterval = TimeSpan.FromHours(24);

        private static Mutex? _mutex;
        private IPlatformServices? _platformServices;
        private SettingsManager _settingsManager = new();
        private KeyboardLayoutConverter _converter = new();
        private MainWindow? _settingsWindow;
        private DispatcherTimer? _updateTimer;
        private bool _updateInProgress;

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

                    // Remove files left by a previous self-update (old instance is closed by now)
                    UpdateService.CleanupLeftovers();

                    // Load settings
                    _settingsManager.LoadSettings();
                    LogDebug("Settings loaded");

                    // Keep the Windows startup entry in sync with the saved setting
                    // (also re-points it at the current exe if the file was moved)
                    StartupManager.Apply(_settingsManager.Settings.RunAtStartup);

                    // Create platform services
                    _platformServices = PlatformServiceFactory.Create();
                    LogDebug("Platform services created");

                    // Register global hotkey
                    RegisterHotkey();
                    LogDebug("Hotkey registration attempted");

                    // Update tray icon tooltip with version
                    var trayIcons = TrayIcon.GetIcons(this);
                    if (trayIcons?.Count > 0)
                    {
                        trayIcons[0].ToolTipText = $"Keyboard Layout Fixer v{AppVersion}";
                        UpdateToggleMenuHeader(trayIcons[0]);
                    }

                    // Periodic update check
                    StartUpdateTimer();

                    // Tray-only app: no main window, shutdown only on explicit request
                    desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                    // Open settings on first run
                    if (_settingsManager.IsFirstRun)
                        Dispatcher.UIThread.Post(ShowSettings);

                    desktop.ShutdownRequested += (_, _) =>
                    {
                        _updateTimer?.Stop();
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

        private static string GetAppVersion()
        {
            var v = typeof(App).Assembly.GetName().Version;
            return v == null ? "0.0.0" : $"{v.Major}.{v.Minor}.{Math.Max(v.Build, 0)}";
        }

        // --- Auto-update ---

        private void StartUpdateTimer()
        {
            if (!UpdateService.CanSelfUpdate)
            {
                LogDebug("Auto-update unavailable (not a published single-file exe)");
                return;
            }

            _updateTimer = new DispatcherTimer { Interval = FirstUpdateCheckDelay };
            _updateTimer.Tick += async (_, _) =>
            {
                _updateTimer.Interval = UpdateCheckInterval;
                await CheckForUpdatesAsync();
            };
            _updateTimer.Start();
        }

        /// <param name="manual">
        /// True when the user asked (tray menu / Settings): ignores the AutoUpdate setting and
        /// always reports the result. Timer checks stay silent unless an update is found.
        /// </param>
        private async Task CheckForUpdatesAsync(bool manual = false)
        {
            // The setting is read on every tick, so toggling it in Settings applies without a restart
            if (_updateInProgress || (!manual && !_settingsManager.Settings.AutoUpdate))
                return;

            if (manual && !UpdateService.CanSelfUpdate)
            {
                await ShowUpdateMessageAsync(
                    $"This build (v{AppVersion}) can't update itself.\nDownload the latest version from:\n{UpdateService.LatestReleaseUrl}",
                    Icon.Info);
                return;
            }

            _updateInProgress = true;
            try
            {
                var updater = new UpdateService(AppVersion, LogDebug);
                var latest = await updater.GetLatestReleaseAsync();
                if (latest == null)
                {
                    LogDebug("Update check: no release info");
                    if (manual)
                        await ShowUpdateMessageAsync(
                            "Couldn't check for updates. Check your internet connection and try again.",
                            Icon.Warning);
                    return;
                }
                if (!UpdateService.IsNewer(latest.Version, Version.Parse(AppVersion)))
                {
                    LogDebug($"Update check: up to date (current {AppVersion}, latest {latest.Tag})");
                    if (manual)
                        await ShowUpdateMessageAsync($"You're on the latest version (v{AppVersion}).", Icon.Success);
                    return;
                }

                LogDebug($"Update available: {latest.Tag}");
                // The window handles Update now / Later, progress and errors itself
                var window = new UpdateWindow(latest, AppVersion, updater);
                bool installed = await window.ShowAndWaitAsync();

                if (installed)
                    ExitApplication();
                else
                    LogDebug("Update window closed without installing");
            }
            catch (Exception ex)
            {
                LogDebug($"Update check error: {ex}");
            }
            finally
            {
                _updateInProgress = false;
            }
        }

        private static Task ShowUpdateMessageAsync(string message, Icon icon) =>
            MessageBoxManager.GetMessageBoxStandard("Check for Updates", message, ButtonEnum.Ok, icon).ShowAsync();

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
            if (!_settingsManager.Settings.IsEnabled)
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

        private async void CheckUpdates_Click(object? sender, EventArgs e)
        {
            await CheckForUpdatesAsync(manual: true);
        }

        private void Toggle_Click(object? sender, EventArgs e)
        {
            var settings = _settingsManager.Settings;
            settings.IsEnabled = !settings.IsEnabled;
            _settingsManager.SaveSettings();
            if (sender is NativeMenuItem menuItem)
            {
                menuItem.Header = settings.IsEnabled ? "Disable" : "Enable";
            }
            LogDebug($"App {(settings.IsEnabled ? "enabled" : "disabled")}");
        }

        private void UpdateToggleMenuHeader(TrayIcon trayIcon)
        {
            var toggleItem = trayIcon.Menu?.Items
                .OfType<NativeMenuItem>()
                .FirstOrDefault(i => i.Header is "Disable" or "Enable");
            if (toggleItem != null)
                toggleItem.Header = _settingsManager.Settings.IsEnabled ? "Disable" : "Enable";
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
                _settingsWindow.CheckForUpdatesRequested += async (_, _) => await CheckForUpdatesAsync(manual: true);
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
