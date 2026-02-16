# Quick Start Guide

## Installation

1. **Download or Build** the application (see README.md for build instructions)
2. **Run** `KeyboardLayoutFixer.exe`
3. The app will minimize to the **system tray** (notification area)

## Basic Usage

### Example Scenario 1: Wrong Keyboard Layout
You typed: `akyn` (in English keyboard)
You meant: `שלום` (in Hebrew)

**Solution:**
1. Select the text `akyn`
2. Press `Ctrl+Q`
3. Text is automatically replaced with `שלום`

### Example Scenario 2: Entire Textbox
You typed a whole paragraph in the wrong layout:

```
rypv whc,u nra eyua fso hfnku sjrka
```

**Solution:**
1. Click in the textbox (don't select anything)
2. Press `Ctrl+Q`
3. All text is converted to Hebrew:
```
תקטן אחמפו הנש צטשש גדם יגהלו דחנלש
```

## Settings Configuration

### Access Settings
- Right-click tray icon → **Settings**
- Or double-click the tray icon

### Change Hotkey
1. Open Settings
2. Check desired modifiers (Ctrl, Alt, Shift, Win)
3. Select key from dropdown (A-Z, F1-F12, etc.)
4. Click **Save**

**Examples:**
- `Ctrl+Shift+L`
- `Alt+Q`
- `Ctrl+Alt+K`
- `Win+F1`

### Set Character Limit
Prevents accidental conversion of large documents.

**Example:**
- Set to `20` → Only texts under 20 characters will convert
- Set to `0` → No limit (convert any length)

### Mixed Text Modes

#### Toggle All (Default)
Converts everything simultaneously.

**Input:** `hello שלום world`
**Output:** `יקממם akyn עםרמג`

#### English to Hebrew Only
Only converts English characters.

**Input:** `hello שלום world`
**Output:** `יקממם שלום עםרמג`

#### Hebrew to English Only
Only converts Hebrew characters.

**Input:** `hello שלום world`
**Output:** `hello akyn world`

## Tips & Tricks

### 1. Quick Correction
- Type in wrong layout
- Immediately press hotkey
- Continue typing

### 2. Partial Conversion
- Select only the wrong part
- Press hotkey
- Rest remains unchanged

### 3. Test Before Using
- Open Notepad
- Type test text: `test` or `akyn`
- Press hotkey to verify it works

### 4. Avoid Large Text
- Set a character limit (e.g., 100)
- Prevents accidentally converting entire documents

## Common Use Cases

### Email Writing
Accidentally typed Hebrew when composing English email:
1. Select the Hebrew text
2. Press hotkey
3. Continue composing

### Chat Messages
Quick message fixes in WhatsApp, Telegram, Discord:
1. Press hotkey (converts entire message box)
2. Send corrected message

### Code Comments
Typed Hebrew comment in English codebase:
1. Select comment line
2. Press hotkey
3. Comment is now in English

### Form Filling
Wrong layout in web form field:
1. Click in field (or select text)
2. Press hotkey
3. Field content corrected

## Troubleshooting

### Hotkey Not Working?
- **Conflict**: Another app uses same hotkey → Change in Settings
- **Admin App**: Some apps need admin rights → Run as administrator
- **Wrong Focus**: Ensure cursor is in text field

### Wrong Conversion?
- **Mixed Text**: Check Mixed Text Mode in Settings
- **Special Chars**: Some symbols don't convert (numbers, punctuation)

### Settings Not Saving?
- Close Settings with **Save** button (not X)
- Check file permissions in `%APPDATA%\KeyboardLayoutFixer\`

## Keyboard Layout Reference

| English | Hebrew | English | Hebrew |
|---------|--------|---------|--------|
| q       | /      | a       | ש      |
| w       | '      | s       | ד      |
| e       | ק      | d       | ג      |
| r       | ר      | f       | כ      |
| t       | א      | g       | ע      |
| y       | ט      | h       | י      |
| u       | ו      | j       | ח      |
| i       | ן      | k       | ל      |
| o       | ם      | l       | ך      |
| p       | פ      | ;       | ף      |

| English | Hebrew | English | Hebrew |
|---------|--------|---------|--------|
| z       | ז      | ,       | ת      |
| x       | ס      | .       | ץ      |
| c       | ב      | /       | .      |
| v       | ה      |         |        |
| b       | נ      |         |        |
| n       | מ      |         |        |
| m       | ץ      |         |        |

## Exit Application

1. Right-click system tray icon
2. Click **Exit**

---

**Happy typing!** 🚀
