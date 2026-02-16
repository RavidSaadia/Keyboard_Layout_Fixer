# Keyboard Layout Fixer

A Windows utility application that corrects keyboard layout typing mistakes between English and Hebrew. Have you ever typed in the wrong keyboard layout? This app fixes that with a simple hotkey!

## Features

### 🎯 Core Functionality
- **Global Hotkey**: Trigger text conversion from anywhere with a customizable hotkey (default: `Ctrl+Q`)
- **Smart Text Detection**: Automatically detects whether to convert selected text or entire textbox
- **Bidirectional Conversion**: Converts English ↔ Hebrew keyboard layouts
- **System Tray Operation**: Runs completely in the background with no taskbar presence

### ⚙️ Configurable Settings
- **Custom Hotkey**: Choose your own key combination (Ctrl/Alt/Shift/Win + Key)
- **Character Limit**: Set a maximum character count to prevent accidental conversions of large texts
- **Mixed Text Handling**: Three modes for text containing both languages:
  - **Toggle All**: Convert both English→Hebrew AND Hebrew→English simultaneously
  - **English to Hebrew Only**: Convert English characters, leave Hebrew unchanged
  - **Hebrew to English Only**: Convert Hebrew characters, leave English unchanged

### 🔒 Smart & Safe
- Non-intrusive: Only acts when you explicitly trigger the hotkey
- Preserves your clipboard content
- Works across all Windows applications
- Silently aborts if character limit is exceeded

## How It Works

1. Type text in the wrong keyboard layout
2. Select the text (or leave it unselected to convert the entire textbox)
3. Press your configured hotkey (default: `Ctrl+Q`)
4. The text is automatically converted and replaced!

**Technical Implementation**: The app uses a clipboard-based approach for maximum compatibility:
- Captures text via `Ctrl+C`
- Converts characters using keyboard layout mapping
- Pastes corrected text via `Ctrl+V`
- Restores original clipboard content

## Character Mapping

The app uses the standard Hebrew keyboard layout mapping:

```
English:  q w e r t y u i o p a s d f g h j k l z x c v b n m
Hebrew:   / ' ק ר א ט ו ן ם פ ש ד ג כ ע י ח ל ך ז ס ב ה נ מ ץ
```

## System Requirements

- **OS**: Windows 10 or later
- **.NET**: .NET 8.0 Runtime or SDK
- **Frameworks**: WPF (included with .NET)

## Building the Application

### Prerequisites
1. Install [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. (Optional) Visual Studio 2022 or JetBrains Rider

### Build Instructions

#### Using Command Line:
```bash
# Navigate to the project directory
cd KeyboardLayoutFixer

# Restore dependencies
dotnet restore

# Build the application
dotnet build --configuration Release

# Run the application
dotnet run --configuration Release
```

#### Create a Standalone Executable:
```bash
# Publish as a single-file executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# The executable will be in: bin\Release\net8.0-windows\win-x64\publish\KeyboardLayoutFixer.exe
```

#### Using Visual Studio:
1. Open `KeyboardLayoutFixer.csproj` in Visual Studio
2. Select **Release** configuration
3. Build → Build Solution (or press `Ctrl+Shift+B`)
4. Run with `F5` or Build → Publish for distribution

## Installation & Usage

### Running the Application
1. Build or download the executable
2. Run `KeyboardLayoutFixer.exe`
3. The app will minimize to the system tray (look for the icon in the notification area)

### Accessing Settings
- **Right-click** the system tray icon → **Settings**
- Or **double-click** the system tray icon

### Configuring the Hotkey
1. Open Settings
2. Select modifier keys (Ctrl, Alt, Shift, Win)
3. Choose the key from the dropdown
4. Click **Save**

### Setting Character Limit
1. Open Settings
2. Enter a number in the "Character Limit" field (0 = unlimited)
3. Click **Save**

### Choosing Mixed Text Mode
1. Open Settings
2. Select one of the three radio button options
3. Click **Save**

## Project Structure

```
KeyboardLayoutFixer/
├── App.xaml                          # Application definition
├── App.xaml.cs                       # Application logic & tray icon management
├── MainWindow.xaml                   # Settings window UI
├── MainWindow.xaml.cs               # Settings window logic
├── GlobalHotkey.cs                   # Windows API hotkey registration
├── KeyboardLayoutConverter.cs        # Character conversion logic
├── SettingsManager.cs               # Configuration persistence
├── KeyboardLayoutFixer.csproj       # Project file
└── README.md                         # This file
```

## Settings File Location

Settings are automatically saved to:
```
%APPDATA%\KeyboardLayoutFixer\settings.json
```

Example settings file:
```json
{
  "HotkeyModifiers": 2,
  "HotkeyKey": 50,
  "MaxCharacterLimit": 0,
  "MixedTextMode": 0
}
```

## Troubleshooting

### Hotkey Not Working
- **Conflict with other applications**: Try changing the hotkey in Settings
- **Administrator rights**: Some applications require administrator privileges to receive hotkeys
- **Antivirus blocking**: Add an exception for KeyboardLayoutFixer.exe

### Text Not Converting
- **Check character limit**: If text exceeds your limit, conversion is aborted
- **Clipboard access**: Ensure the app has permission to access clipboard
- **Focus issues**: Make sure a text input field is focused when pressing the hotkey

### Settings Not Saving
- **Permissions**: Ensure write access to `%APPDATA%\KeyboardLayoutFixer\`
- **Corrupted file**: Delete `settings.json` to reset to defaults

## Dependencies

- **Newtonsoft.Json** (13.0.3) - Settings serialization
- **.NET 8.0** - Runtime framework
- **Windows.Forms** - System tray icon
- **WPF** - Settings UI

## Technical Details

### Global Hotkey Implementation
The app uses Windows API (`RegisterHotKey`/`UnregisterHotKey`) to register system-wide keyboard shortcuts that work across all applications.

### Clipboard-Based Text Capture
Since reading directly from arbitrary Windows text controls is unreliable, the app uses a clipboard-based approach:
1. Simulates `Ctrl+C` to copy selected/all text
2. Reads from clipboard
3. Converts text
4. Places result in clipboard
5. Simulates `Ctrl+V` to paste
6. Restores original clipboard content

This ensures compatibility with virtually all Windows applications.

## License

This project is provided as-is for educational and personal use.

## Contributing

Feel free to submit issues and enhancement requests!

## Author

Created as a complete Windows utility solution for keyboard layout correction.

---

**Enjoy seamless typing across keyboard layouts!** 🎉
