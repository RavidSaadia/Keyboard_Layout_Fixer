using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using KeyboardLayoutFixer.Services;

namespace KeyboardLayoutFixer.Views
{
    /// <summary>
    /// "Update available" window: offer → download progress → (on failure) error with manual-download link.
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private readonly UpdateInfo? _update;
        private readonly UpdateService? _updater;
        private readonly TaskCompletionSource<bool> _result = new();
        private bool _installing;
        private bool _installed;

        public UpdateWindow(UpdateInfo update, string currentVersion, UpdateService updater)
        {
            InitializeComponent();
            _update = update;
            _updater = updater;

            CurrentVersionText.Text = $"v{currentVersion}";
            NewVersionText.Text = $"v{update.Version}";
            Closed += (_, _) => _result.TrySetResult(_installed);
        }

        // Parameterless constructor for XAML designer
        public UpdateWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the window and completes when it closes. True = the new version was installed
        /// and started, so the caller should shut this process down.
        /// </summary>
        public Task<bool> ShowAndWaitAsync()
        {
            Show();
            return _result.Task;
        }

        private async void UpdateButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_update == null || _updater == null || _installing) return;

            ShowProgress(0);
            try
            {
                var progress = new Progress<double>(p => Dispatcher.UIThread.Post(() => ShowProgress(p)));
                await _updater.InstallAsync(_update, progress);

                _installed = true;
                ProgressText.Text = "Done! Restarting…";
                DownloadProgress.Value = 100;
                await Task.Delay(600);
                _installing = false;
                Close();
            }
            catch (UpdateException ex)
            {
                ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                ShowError($"Something went wrong: {ex.Message}");
            }
        }

        private void LaterButton_Click(object? sender, RoutedEventArgs e)
        {
            if (!_installing) Close();
        }

        private void ManualButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(UpdateService.LatestReleaseUrl)
                {
                    UseShellExecute = true
                });
            }
            catch { }
            Close();
        }

        private void ShowProgress(double fraction)
        {
            _installing = true;
            HeadlineText.Text = "Updating…";
            SubtitleText.Text = "Hang tight, this only takes a moment.";
            PerksRow.IsVisible = false;
            ProgressPanel.IsVisible = true;
            Footer.IsVisible = false;
            CloseButton.IsVisible = false;

            int percent = (int)Math.Round(Math.Clamp(fraction, 0, 1) * 100);
            DownloadProgress.Value = percent;
            ProgressText.Text = percent < 100 ? $"Downloading… {percent}%" : "Installing…";
        }

        private void ShowError(string message)
        {
            _installing = false;
            HeroArrow.IsVisible = false;
            HeroError.IsVisible = true;
            HeadlineText.Text = "Update didn't finish";
            SubtitleText.Text = $"{message}\nYour current version still works. You can grab the new one from GitHub.";
            VersionRow.IsVisible = false;
            PerksRow.IsVisible = false;
            ProgressPanel.IsVisible = false;
            Footer.IsVisible = true;
            CloseButton.IsVisible = true;
            UpdateButton.IsVisible = false;
            ManualButton.IsVisible = true;
            ManualButton.Classes.Remove("ghost");
            ManualButton.Classes.Add("cta");
            LaterButton.Content = "Close";
            SubtitleText.Margin = new Avalonia.Thickness(28, 6, 28, 24);
        }

        private void Window_Closing(object? sender, WindowClosingEventArgs e)
        {
            // Don't let the window close halfway through swapping the exe
            if (_installing) e.Cancel = true;
        }

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        }
    }
}
