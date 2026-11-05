# Keyboard Layout Fixer

Typed a whole sentence with the wrong keyboard language? Press **`Alt+Q`** and it's fixed.

Keyboard Layout Fixer is a small Windows tray app that converts text typed in the wrong layout between **English (QWERTY)** and **Hebrew**, in any application: browsers, Outlook, Teams, Word, Slack, code editors, and so on.

```
You typed:   akuo guko        (keyboard was on English)
You meant:   שלום עולם
Press Alt+Q → שלום עולם
```

---

## Quick start (just the .exe)

1. Download `KeyboardLayoutFixer.exe` from the [latest release](https://github.com/RavidSaadia/Keyboard_Layout_Fixer/releases/latest).
2. Put it somewhere permanent, e.g. `C:\Tools\KeyboardLayoutFixer.exe`.
   > With **Run at Windows startup** on (the default), Windows starts the exe from the folder it was in the last time it ran. If you move it, run it once from the new location so the startup entry is updated.
3. Double-click it. Nothing opens: the app sits in the **system tray** (bottom right, near the clock; check the `^` overflow arrow).
4. Type something in the wrong layout, then press **`Alt+Q`**.

No installation and no .NET runtime needed. The exe is self-contained.

---

## How to use

| You want to fix… | Do this |
|---|---|
| A specific word or phrase | Select it → press `Alt+Q` |
| The whole text field | Click inside the field, select nothing → press `Alt+Q` |

After converting, the app also switches your keyboard language (`Alt+Shift`) by default, so you can keep typing in the correct language.

### Tray menu

Right-click the tray icon:

| Item | What it does |
|---|---|
| **Settings** | Opens the settings window (clicking the icon does the same) |
| **Disable / Enable** | Pauses or resumes the hotkey. Remembered after restart |
| **Restart** | Restarts the app |
| **Exit** | Closes the app |

---

## Settings

Open **Settings** from the tray icon, change what you want, and click **Save**.

| Setting | Default | Description |
|---|---|---|
| **Global hotkey** | `Alt+Q` | Any combination of Ctrl / Alt / Shift / Win + a letter, F1–F12, Space, Enter or Tab |
| **Character limit** | `200` | If the captured text is longer than this, nothing is converted. Protects you from converting a whole document by accident. `0` = unlimited |
| **Convert uppercase (CAPS) letters** | Off | When off, `A–Z` are left alone and only lowercase letters are converted, so acronyms like `API` or `PR` survive |
| **Switch keyboard language after conversion** | On | Sends `Alt+Shift` after converting so your keyboard matches the fixed text |
| **Run at Windows startup** | On | Starts the app automatically when you log in |
| **Check for updates automatically** | On | Looks for a newer version on GitHub and offers to install it (see [Updates](#updates)) |
| **Mixed text handling** | Toggle All | See below |

### Mixed text handling

For text that contains both Hebrew and English:

- **Toggle All**: English → Hebrew *and* Hebrew → English at the same time
- **English to Hebrew Only**: converts English characters only, leaves Hebrew untouched
- **Hebrew to English Only**: converts Hebrew characters only, leaves English untouched

### Are my settings kept?

Yes. Settings are saved to your Windows user profile, **not** next to the exe:

```
%APPDATA%\KeyboardLayoutFixer\settings.json
```

They're kept when you close and reopen the app, restart the PC, or replace the exe with a newer version. To reset to defaults, exit the app, delete `settings.json`, and start the app again.

---

## Updates

The app checks GitHub for a newer version about 30 seconds after it starts, and then once a day. When it finds one, it asks:

> A new version of Keyboard Layout Fixer is available. Update now?

- **Yes**: downloads the new exe, checks its SHA-256 checksum, replaces the current exe in the same folder and restarts. Your settings are kept.
- **No**: nothing changes. It asks again at the next check.

To turn this off, uncheck **Check for updates automatically** in Settings. You can always download the latest version manually from the [Releases page](https://github.com/RavidSaadia/Keyboard_Layout_Fixer/releases/latest).

> Versions **1.2.9 and earlier** have no updater. Download 1.3.0 or newer manually once; after that, updates are automatic.

If an update fails (no internet, or the exe is in a folder you can't write to, like `Program Files`), the current version keeps working and you're shown the manual download link.

---

## Character mapping

Standard Israeli Hebrew keyboard layout (SI-1452):

```
English:  q w e r t y u i o p   a s d f g h j k l ;   z x c v b n m , .
Hebrew:   / ' ק ר א ט ו ן ם פ   ש ד ג כ ע י ח ל ך ף   ז ס ב ה נ מ צ ת ץ
```

Other punctuation: `/` ↔ `.`, `'` ↔ `,`, `[` ↔ `]`.

---

## How it works

The app uses the clipboard, which works in almost every Windows application:

1. Waits for you to release the hotkey's modifier keys
2. Saves your current clipboard
3. Sends `Ctrl+C` (or `Ctrl+A` then `Ctrl+C` if nothing was selected)
4. Converts the text character by character
5. Pastes it back with `Ctrl+V`
6. Optionally sends `Alt+Shift` to switch the keyboard language
7. Restores your original clipboard

Only plain text is restored to the clipboard. If you had an image or file copied, it's cleared.

---

## Troubleshooting

| Problem | Fix |
|---|---|
| Hotkey does nothing | Another app may be using the same shortcut. Pick a different one in Settings. Also check that the app isn't **Disabled** in the tray menu |
| Doesn't work in a specific app | Apps running as Administrator can't receive keystrokes from a normal app. Run Keyboard Layout Fixer as Administrator too |
| Nothing converts on long text | Your text is over the **Character limit**. Raise it, or set it to `0` |
| Language doesn't switch afterwards | The app sends `Alt+Shift`. If your Windows uses `Win+Space` or another shortcut, turn off **Switch keyboard language** |
| App doesn't start with Windows | Make sure **Run at Windows startup** is on, then run the exe once from its current location |
| Settings reset unexpectedly | Check that `%APPDATA%\KeyboardLayoutFixer\` is writable (some corporate profiles restrict it) |

A debug log is written to `%APPDATA%\KeyboardLayoutFixer\debug.log`. Include it when reporting an issue.

---

## Uninstall

1. Open Settings → uncheck **Run at Windows startup** → Save
2. Tray menu → **Exit**
3. Delete the exe and the `%APPDATA%\KeyboardLayoutFixer\` folder

---

## For developers

### Tech stack

- .NET 8, [Avalonia UI](https://avaloniaui.net/) 11
- Win32 `RegisterHotKey` / `SendInput` for the global hotkey and keystroke simulation
- Newtonsoft.Json for settings
- macOS support is scaffolded (`Platform/macOS`) but not implemented yet

### Build and run

```bash
dotnet restore
dotnet run -c Release
```

> Running from source with **Run at Windows startup** enabled points the startup entry at your `bin\` folder. Turn it off in Settings if you don't want that.

### Publish a standalone single-file exe

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Output: `bin\Release\net8.0\win-x64\publish\KeyboardLayoutFixer.exe`

Or run `build.bat`, which builds and optionally publishes.

### Project structure

```
App.axaml / App.axaml.cs          Tray icon, hotkey handling, conversion flow
Program.cs                        Entry point
Views/MainWindow.axaml(.cs)       Settings window
Themes/DesignSystem.axaml         UI styles
Models/AppSettings.cs             Settings and their defaults
Models/Hotkey*.cs, MixedTextMode  Enums
Services/KeyboardLayoutConverter  English ↔ Hebrew character mapping
Services/SettingsManager.cs       Load/save settings.json (atomic writes)
Services/StartupManager.cs        "Run at Windows startup" registry entry
Services/UpdateService.cs         Self-update from GitHub Releases
tests/                            xUnit tests (dotnet test tests/KeyboardLayoutFixer.Tests)
.github/workflows/release.yml     Builds the exe and creates a GitHub Release on version tags
Platform/                         OS abstraction (Windows implemented, macOS stub)
```

### Releasing

Releases are built by GitHub Actions ([release.yml](.github/workflows/release.yml)):

1. Bump `<Version>` in [KeyboardLayoutFixer.csproj](KeyboardLayoutFixer.csproj) and commit.
2. Tag and push:
   ```bash
   git tag v1.3.0
   git push origin v1.3.0
   ```
3. The workflow runs the tests, publishes the single-file exe with the version taken from the tag, and creates a GitHub Release with `KeyboardLayoutFixer.exe` and `KeyboardLayoutFixer.exe.sha256`.

Running apps pick up the release at their next update check. Keep both asset names unchanged, because the updater looks for them by name. Only plain `vX.Y.Z` tags count as updates (`v1.4.0-beta` is ignored).

Auto-update only runs in the published single-file exe. Builds started with `dotnet run` or from `bin\` never replace themselves.

---

## License

See [LICENSE](LICENSE).
