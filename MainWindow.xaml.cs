using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace KeyboardLayoutFixer
{
    /// <summary>
    /// Settings window for configuring the application
    /// </summary>
    public partial class MainWindow : Window
    {
        private SettingsManager _settingsManager;
        public event EventHandler? HotkeyChanged;

        public MainWindow(SettingsManager settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;

            InitializeKeyComboBox();
            LoadSettings();

            // Subscribe to checkbox changes
            CtrlCheckBox.Checked += HotkeyChanged_Event;
            CtrlCheckBox.Unchecked += HotkeyChanged_Event;
            AltCheckBox.Checked += HotkeyChanged_Event;
            AltCheckBox.Unchecked += HotkeyChanged_Event;
            ShiftCheckBox.Checked += HotkeyChanged_Event;
            ShiftCheckBox.Unchecked += HotkeyChanged_Event;
            WinCheckBox.Checked += HotkeyChanged_Event;
            WinCheckBox.Unchecked += HotkeyChanged_Event;
        }

        private void InitializeKeyComboBox()
        {
            // Add common keys to the combo box
            var keys = new[]
            {
                Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P,
                Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L,
                Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M,
                Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
                Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
                Key.Space, Key.Enter, Key.Tab
            };

            foreach (var key in keys)
            {
                KeyComboBox.Items.Add(key);
            }
        }

        private void LoadSettings()
        {
            var settings = _settingsManager.Settings;

            // Load hotkey modifiers
            CtrlCheckBox.IsChecked = settings.HotkeyModifiers.HasFlag(ModifierKeys.Control);
            AltCheckBox.IsChecked = settings.HotkeyModifiers.HasFlag(ModifierKeys.Alt);
            ShiftCheckBox.IsChecked = settings.HotkeyModifiers.HasFlag(ModifierKeys.Shift);
            WinCheckBox.IsChecked = settings.HotkeyModifiers.HasFlag(ModifierKeys.Win);

            // Load hotkey key
            KeyComboBox.SelectedItem = settings.HotkeyKey;

            // Load character limit
            CharLimitTextBox.Text = settings.MaxCharacterLimit.ToString();

            // Load replace caps
            ReplaceCapsCheckBox.IsChecked = settings.ReplaceCaps;

            // Load mixed text mode
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate at least one modifier is selected
            if (!CtrlCheckBox.IsChecked.GetValueOrDefault() &&
                !AltCheckBox.IsChecked.GetValueOrDefault() &&
                !ShiftCheckBox.IsChecked.GetValueOrDefault() &&
                !WinCheckBox.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show(
                    "Please select at least one modifier key (Ctrl, Alt, Shift, or Win).",
                    "Invalid Hotkey",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // Validate key is selected
            if (KeyComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select a key for the hotkey.",
                    "Invalid Hotkey",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            try
            {
                var settings = _settingsManager.Settings;
                bool hotkeyChanged = false;

                // Save hotkey modifiers
                ModifierKeys newModifiers = ModifierKeys.None;
                if (CtrlCheckBox.IsChecked.GetValueOrDefault())
                    newModifiers |= ModifierKeys.Control;
                if (AltCheckBox.IsChecked.GetValueOrDefault())
                    newModifiers |= ModifierKeys.Alt;
                if (ShiftCheckBox.IsChecked.GetValueOrDefault())
                    newModifiers |= ModifierKeys.Shift;
                if (WinCheckBox.IsChecked.GetValueOrDefault())
                    newModifiers |= ModifierKeys.Win;

                if (settings.HotkeyModifiers != newModifiers)
                {
                    settings.HotkeyModifiers = newModifiers;
                    hotkeyChanged = true;
                }

                // Save hotkey key
                Key newKey = (Key)KeyComboBox.SelectedItem;
                if (settings.HotkeyKey != newKey)
                {
                    settings.HotkeyKey = newKey;
                    hotkeyChanged = true;
                }

                // Save character limit
                if (int.TryParse(CharLimitTextBox.Text, out int limit))
                {
                    settings.MaxCharacterLimit = Math.Max(0, limit);
                }

                // Save replace caps
                settings.ReplaceCaps = ReplaceCapsCheckBox.IsChecked.GetValueOrDefault();

                // Save mixed text mode
                if (ToggleAllRadio.IsChecked.GetValueOrDefault())
                    settings.MixedTextMode = MixedTextMode.ToggleAll;
                else if (EnglishToHebrewRadio.IsChecked.GetValueOrDefault())
                    settings.MixedTextMode = MixedTextMode.EnglishToHebrewOnly;
                else if (HebrewToEnglishRadio.IsChecked.GetValueOrDefault())
                    settings.MixedTextMode = MixedTextMode.HebrewToEnglishOnly;

                // Save to disk
                _settingsManager.SaveSettings();

                // Notify if hotkey changed
                if (hotkeyChanged)
                {
                    HotkeyChanged?.Invoke(this, EventArgs.Empty);
                }

                MessageBox.Show(
                    "Settings saved successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving settings: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings(); // Reload original settings
            this.Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Instead of closing, just hide the window
            e.Cancel = true;
            this.Hide();
        }

        private void HotkeyChanged_Event(object sender, RoutedEventArgs e)
        {
            UpdateHotkeyDisplay();
        }

        private void KeyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateHotkeyDisplay();
        }

        private void UpdateHotkeyDisplay()
        {
            var parts = new List<string>();

            if (CtrlCheckBox.IsChecked.GetValueOrDefault())
                parts.Add("Ctrl");
            if (AltCheckBox.IsChecked.GetValueOrDefault())
                parts.Add("Alt");
            if (ShiftCheckBox.IsChecked.GetValueOrDefault())
                parts.Add("Shift");
            if (WinCheckBox.IsChecked.GetValueOrDefault())
                parts.Add("Win");

            if (KeyComboBox.SelectedItem != null)
                parts.Add(KeyComboBox.SelectedItem.ToString()!);

            HotkeyDisplay.Text = parts.Count > 0 ? string.Join("+", parts) : "(None)";
        }

        private void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Only allow numeric input
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
