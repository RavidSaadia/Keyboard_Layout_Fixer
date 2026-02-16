using System.Runtime.InteropServices;
using System.Text;
using KeyboardLayoutFixer.Models;

namespace KeyboardLayoutFixer.Platform.Windows
{
    /// <summary>
    /// Windows implementation of platform services using Win32 APIs.
    /// </summary>
    public class WindowsPlatformServices : IPlatformServices
    {
        #region P/Invoke Declarations

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        // Window creation for message-only hotkey window
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle, string lpClassName, string lpWindowName,
            uint dwStyle, int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll")]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // Clipboard P/Invoke
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region Win32 Structures

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint Type;
            public INPUTUNION Union;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)] public MOUSEINPUT Mouse;
            [FieldOffset(0)] public KEYBDINPUT Keyboard;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort VirtualKey;
            public ushort ScanCode;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WNDCLASS
        {
            public uint style;
            public WndProcDelegate lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string? lpszMenuName;
            public string lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int pt_x;
            public int pt_y;
        }

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int WM_HOTKEY = 0x0312;
        private const int WM_QUIT = 0x0012;
        private const int HOTKEY_ID = 9000;
        private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

        // Virtual key codes
        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_SHIFT = 0x10;
        private const ushort VK_MENU = 0x12; // Alt
        private const ushort VK_LWIN = 0x5B;
        private const ushort VK_RWIN = 0x5C;
        private const ushort VK_C = 0x43;
        private const ushort VK_V = 0x56;
        private const ushort VK_A = 0x41;

        // Clipboard constants
        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVEABLE = 0x0002;

        #endregion

        private IntPtr _messageWindow;
        private Thread? _messageThread;
        private Action? _hotkeyCallback;
        private bool _registered;
        private bool _disposed;
        private WndProcDelegate? _wndProc; // prevent GC

        public void RegisterHotkey(HotkeyModifiers modifiers, HotkeyKey key, Action onPressed)
        {
            if (_registered)
                UnregisterHotkey();

            _hotkeyCallback = onPressed;
            uint vk = key.ToVirtualKeyCode();
            uint mods = (uint)modifiers;

            // Create a message-only window on a dedicated thread for hotkey messages
            var readySignal = new ManualResetEventSlim(false);
            Exception? threadException = null;

            _messageThread = new Thread(() =>
            {
                try
                {
                    _wndProc = WndProc;
                    var hInstance = GetModuleHandle(null);
                    var className = "KBLayoutFixerHotkey_" + Guid.NewGuid().ToString("N")[..8];

                    var wc = new WNDCLASS
                    {
                        lpfnWndProc = _wndProc,
                        hInstance = hInstance,
                        lpszClassName = className
                    };

                    RegisterClass(ref wc);

                    _messageWindow = CreateWindowEx(
                        0, className, "KBLayoutFixer Hotkey",
                        0, 0, 0, 0, 0,
                        HWND_MESSAGE, IntPtr.Zero, hInstance, IntPtr.Zero);

                    if (_messageWindow == IntPtr.Zero)
                    {
                        threadException = new InvalidOperationException(
                            $"Failed to create message window. Error: {Marshal.GetLastWin32Error()}");
                        readySignal.Set();
                        return;
                    }

                    if (!RegisterHotKey(_messageWindow, HOTKEY_ID, mods, vk))
                    {
                        int error = Marshal.GetLastWin32Error();
                        DestroyWindow(_messageWindow);
                        _messageWindow = IntPtr.Zero;
                        threadException = new InvalidOperationException(
                            $"Failed to register hotkey. Error code: {error}");
                        readySignal.Set();
                        return;
                    }

                    _registered = true;
                    readySignal.Set();

                    // Message pump
                    while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
                    {
                        TranslateMessage(ref msg);
                        DispatchMessage(ref msg);
                    }
                }
                catch (Exception ex)
                {
                    threadException = ex;
                    readySignal.Set();
                }
            });

            _messageThread.IsBackground = true;
            if (OperatingSystem.IsWindows())
                _messageThread.SetApartmentState(ApartmentState.STA);
            _messageThread.Start();

            readySignal.Wait(5000);

            if (threadException != null)
                throw threadException;
        }

        public void UnregisterHotkey()
        {
            if (!_registered)
                return;

            if (_messageWindow != IntPtr.Zero)
            {
                UnregisterHotKey(_messageWindow, HOTKEY_ID);
                PostMessage(_messageWindow, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            }

            _messageThread?.Join(3000);
            _messageThread = null;
            _registered = false;
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                _hotkeyCallback?.Invoke();
                return IntPtr.Zero;
            }
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        public async Task SimulateCopyAsync()
        {
            SendKeyCombo(VK_CONTROL, VK_C);
            await Task.Delay(100);
        }

        public async Task SimulatePasteAsync()
        {
            SendKeyCombo(VK_CONTROL, VK_V);
            await Task.Delay(300);
        }

        public async Task SimulateSelectAllAsync()
        {
            SendKeyCombo(VK_CONTROL, VK_A);
            await Task.Delay(50);
        }

        public async Task SimulateLanguageSwitchAsync()
        {
            // Alt+Shift for Windows keyboard language switch
            SendKeyCombo(VK_MENU, VK_SHIFT);
            await Task.Delay(50);
        }

        public void WaitForModifierKeysRelease(int timeoutMs = 1000)
        {
            int waited = 0;
            while (waited < timeoutMs)
            {
                bool anyHeld =
                    (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0 ||
                    (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0 ||
                    (GetAsyncKeyState(VK_MENU) & 0x8000) != 0 ||
                    (GetAsyncKeyState(VK_LWIN) & 0x8000) != 0 ||
                    (GetAsyncKeyState(VK_RWIN) & 0x8000) != 0;

                if (!anyHeld) break;
                Thread.Sleep(10);
                waited += 10;
            }
        }

        private void SendKeyCombo(ushort modifierVk, ushort keyVk)
        {
            var inputs = new INPUT[4];
            int size = Marshal.SizeOf<INPUT>();

            // Modifier key down
            inputs[0] = CreateKeyInput(modifierVk, false);
            // Key down
            inputs[1] = CreateKeyInput(keyVk, false);
            // Key up
            inputs[2] = CreateKeyInput(keyVk, true);
            // Modifier key up
            inputs[3] = CreateKeyInput(modifierVk, true);

            uint sent = SendInput(4, inputs, size);
            System.Diagnostics.Debug.WriteLine(
                $"SendInput: sent={sent}/4, cbSize={size}, mod=0x{modifierVk:X2}, key=0x{keyVk:X2}, error={Marshal.GetLastWin32Error()}");
        }

        private INPUT CreateKeyInput(ushort vk, bool keyUp)
        {
            return new INPUT
            {
                Type = INPUT_KEYBOARD,
                Union = new INPUTUNION
                {
                    Keyboard = new KEYBDINPUT
                    {
                        VirtualKey = vk,
                        ScanCode = 0,
                        Flags = keyUp ? KEYEVENTF_KEYUP : 0,
                        Time = 0,
                        ExtraInfo = GetMessageExtraInfo()
                    }
                }
            };
        }

        public string? GetClipboardText()
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        IntPtr hData = GetClipboardData(CF_UNICODETEXT);
                        if (hData == IntPtr.Zero)
                            return null;

                        IntPtr pData = GlobalLock(hData);
                        if (pData == IntPtr.Zero)
                            return null;

                        try
                        {
                            return Marshal.PtrToStringUni(pData);
                        }
                        finally
                        {
                            GlobalUnlock(hData);
                        }
                    }
                    finally
                    {
                        CloseClipboard();
                    }
                }
                Thread.Sleep(10);
            }
            return null;
        }

        public void SetClipboardText(string text)
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        EmptyClipboard();

                        byte[] bytes = Encoding.Unicode.GetBytes(text + "\0");
                        IntPtr hMem = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes.Length);
                        if (hMem == IntPtr.Zero)
                            return;

                        IntPtr pMem = GlobalLock(hMem);
                        if (pMem == IntPtr.Zero)
                            return;

                        Marshal.Copy(bytes, 0, pMem, bytes.Length);
                        GlobalUnlock(hMem);

                        SetClipboardData(CF_UNICODETEXT, hMem);
                        // System owns hMem after SetClipboardData - do not free
                    }
                    finally
                    {
                        CloseClipboard();
                    }
                    return;
                }
                Thread.Sleep(10);
            }
        }

        public void ClearClipboard()
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        EmptyClipboard();
                    }
                    finally
                    {
                        CloseClipboard();
                    }
                    return;
                }
                Thread.Sleep(10);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            UnregisterHotkey();

            if (_messageWindow != IntPtr.Zero)
            {
                DestroyWindow(_messageWindow);
                _messageWindow = IntPtr.Zero;
            }
        }
    }
}
