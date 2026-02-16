using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using KeyboardLayoutFixer.Models;
using KeyboardLayoutFixer.Services;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Text.RegularExpressions;

namespace KeyboardLayoutFixer.Views
{
    public partial class MainWindow : Window
    {
        private SettingsManager _settingsManager;
        public event EventHandler? HotkeyChanged;

        public MainWindow(SettingsManager settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;

            VersionText.Text = $"v{App.AppVersion}";
            InitializeKeyComboBox();
            LoadSettings();

            // Subscribe to checkbox changes
            CtrlCheckBox.IsCheckedChanged += HotkeyChanged_Event;
            AltCheckBox.IsCheckedChanged += HotkeyChanged_Event;
            ShiftCheckBox.IsCheckedChanged += HotkeyChanged_Event;
            WinCheckBox.IsCheckedChanged += HotkeyChanged_Event;

            // Handle numeric-only input for character limit
            CharLimitTextBox.KeyDown += NumericTextBox_KeyDown;

            // Handle Escape to close
            KeyDown += (_, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    LoadSettings();
                    this.Hide();
                }
            };
        }

        // Parameterless constructor for XAML designer
        public MainWindow()
        {
            InitializeComponent();
            _settingsManager = new SettingsManager();
        }

        private void InitializeKeyComboBox()
        {
            var keys = new[]
            {
                HotkeyKey.Q, HotkeyKey.W, HotkeyKey.E, HotkeyKey.R, HotkeyKey.T,
                HotkeyKey.Y, HotkeyKey.U, HotkeyKey.I, HotkeyKey.O, HotkeyKey.P,
                HotkeyKey.A, HotkeyKey.S, HotkeyKey.D, HotkeyKey.F, HotkeyKey.G,
                HotkeyKey.H, HotkeyKey.J, HotkeyKey.K, HotkeyKey.L,
                HotkeyKey.Z, HotkeyKey.X, HotkeyKey.C, HotkeyKey.V, HotkeyKey.B,
                HotkeyKey.N, HotkeyKey.M,
                HotkeyKey.F1, HotkeyKey.F2, HotkeyKey.F3, HotkeyKey.F4,
                HotkeyKey.F5, HotkeyKey.F6, HotkeyKey.F7, HotkeyKey.F8,
                HotkeyKey.F9, HotkeyKey.F10, HotkeyKey.F11, HotkeyKey.F12,
                HotkeyKey.Space, HotkeyKey.Enter, HotkeyKey.Tab
            };

            foreach (var key in keys)
            {
                KeyComboBox.Items.Add(key);
            }
        }

        private void LoadSettings()
        {
            var settings = _settingsManager.Settings;

            CtrlCheckBox.IsChecked = settings.HotkeyModifiers.HasFlag(HotkeyModifiers.Control);
            AltCheckBox.IsChecked = settings.HotkeyModifiers.HasFlag(HotkeyModifiers.Alt);
            ShiftCheckBox.IsChecked = settings.HotkeyModifiers.HasFlag(HotkeyModifiers.Shift);
            WinCheckBox.IsChecked = settings.HotkeyModifiers.HasFlag(HotkeyModifiers.Win);

            KeyComboBox.SelectedItem = settings.HotkeyKey;

            CharLimitTextBox.Text = settings.MaxCharacterLimit.ToString();
            ReplaceCapsCheckBox.IsChecked = settings.ReplaceCaps;
            SwitchLanguageCheckBox.IsChecked = settings.SwitchLanguageAfterConvert;

            switch (settings.MixedTextMode)
            {
                case MixedTextMode.ToggleAll:
                    ToggleAllRadio.IsChecked = true;
                    break;
                case MixedTextMode.EnglishToHebrewOnly:
                    EnglishToHebrewRadio.IsChecked = true;
                    break;
                case MixedTextMode.HebrewToEnglishOnly:
                    HebrewToEnglishRadio.IsChecked = true;
                    break;
            }

            UpdateHotkeyDisplay();
        }

        private async void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            if (!CtrlCheckBox.IsChecked.GetValueOrDefault() &&
                !AltCheckBox.IsChecked.GetValueOrDefault() &&
                !ShiftCheckBox.IsChecked.GetValueOrDefault() &&
                !WinCheckBox.IsChecked.GetValueOrDefault())
            {
                await MessageBoxManager.GetMessageBoxStandard(
                    "Invalid Hotkey",
                    "Please select at least one modifier key (Ctrl, Alt, Shift, or Win).",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning)
                    .ShowWindowDialogAsync(this);
                return;
            }

            if (KeyComboBox.SelectedItem == null)
            {
                await MessageBoxManager.GetMessageBoxStandard(
                    "Invalid Hotkey",
                    "Please select a key for the hotkey.",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning)
                    .ShowWindowDialogAsync(this);
                return;
            }

            try
            {
                var settings = _settingsManager.Settings;
                bool hotkeyChanged = false;

                HotkeyModifiers newModifiers = HotkeyModifiers.None;
                if (CtrlCheckBox.IsChecked.GetValueOrDefault())
                    newModifiers |= HotkeyModifiers.Control;
                if (AltCheckBox.IsChecked.GetValueOrDefault())
                    newModifiers |= HotkeyModifiers.Alt;
                if (ShiftCheckBox.IsChecked.GetValueOrDefault())
                    newModifiers |= HotkeyModifiers.Shift;
                if (WinCheckBox.IsChecked.GetValueOrDefault())
                    newModifiers |= HotkeyModifiers.Win;

                if (settings.HotkeyModifiers != newModifiers)
                {
                    settings.HotkeyModifiers = newModifiers;
                    hotkeyChanged = true;
                }

                HotkeyKey newKey = (HotkeyKey)KeyComboBox.SelectedItem;
                if (settings.HotkeyKey != newKey)
                {
                    settings.HotkeyKey = newKey;
                    hotkeyChanged = true;
                }

                if (int.TryParse(CharLimitTextBox.Text, out int limit))
                {
                    settings.MaxCharacterLimit = Math.Max(0, limit);
                }

                settings.ReplaceCaps = ReplaceCapsCheckBox.IsChecked.GetValueOrDefault();
                settings.SwitchLanguageAfterConvert = SwitchLanguageCheckBox.IsChecked.GetValueOrDefault();

                if (ToggleAllRadio.IsChecked.GetValueOrDefault())
                    settings.MixedTextMode = MixedTextMode.ToggleAll;
                else if (EnglishToHebrewRadio.IsChecked.GetValueOrDefault())
                    settings.MixedTextMode = MixedTextMode.EnglishToHebrewOnly;
                else if (HebrewToEnglishRadio.IsChecked.GetValueOrDefault())
                    settings.MixedTextMode = MixedTextMode.HebrewToEnglishOnly;

                _settingsManager.SaveSettings();

                if (hotkeyChanged)
                {
                    HotkeyChanged?.Invoke(this, EventArgs.Empty);
                }

                await MessageBoxManager.GetMessageBoxStandard(
                    "Success",
                    "Settings saved successfully!",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info)
                    .ShowWindowDialogAsync(this);

                this.Hide();
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard(
                    "Error",
                    $"Error saving settings: {ex.Message}",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error)
                    .ShowWindowDialogAsync(this);
            }
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            LoadSettings();
            this.Hide();
        }

        private void Window_Closing(object? sender, WindowClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void HotkeyChanged_Event(object? sender, RoutedEventArgs e)
        {
            UpdateHotkeyDisplay();
        }

        private void KeyComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateHotkeyDisplay();
        }

        private void UpdateHotkeyDisplay()
        {
            if (HotkeyDisplay == null) return;

            var parts = new List<string>();

            if (CtrlCheckBox.IsChecked.GetValueOrDefault())
                parts.Add("Ctrl");
            if (AltCheckBox.IsChecked.GetValueOrDefault())
                parts.Add("Alt");
            if (ShiftCheckBox.IsChecked.GetValueOrDefault())
                parts.Add("Shift");
            if (WinCheckBox.IsChecked.GetValueOrDefault())
                parts.Add(OperatingSystem.IsMacOS() ? "Cmd" : "Win");

            if (KeyComboBox.SelectedItem != null)
                parts.Add(KeyComboBox.SelectedItem.ToString()!);

            HotkeyDisplay.Text = parts.Count > 0 ? string.Join("+", parts) : "(None)";
        }

        private void NumericTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            // Allow digits, backspace, delete, and navigation keys
            bool isDigit = e.Key >= Key.D0 && e.Key <= Key.D9;
            bool isNumPad = e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9;
            bool isControl = e.Key == Key.Back || e.Key == Key.Delete ||
                           e.Key == Key.Left || e.Key == Key.Right ||
                           e.Key == Key.Home || e.Key == Key.End ||
                           e.Key == Key.Tab;

            if (!isDigit && !isNumPad && !isControl)
            {
                e.Handled = true;
            }
        }

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
