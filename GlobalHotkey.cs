using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Input;

namespace KeyboardLayoutFixer
{
    /// <summary>
    /// Manages global keyboard hotkeys using Windows API
    /// </summary>
    public class GlobalHotkey : IDisposable
    {
        // Windows API imports
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;

        private IntPtr _windowHandle;
        private HwndSource? _source;
        private Action _callback;
        private uint _modifiers;
        private uint _key;
        private bool _registered;

        public GlobalHotkey(ModifierKeys modifiers, Key key, Action callback)
        {
            _callback = callback;
            _modifiers = (uint)modifiers;
            _key = (uint)KeyInterop.VirtualKeyFromKey(key);
            _registered = false;
        }

        /// <summary>
        /// Registers the hotkey
        /// </summary>
        public void Register()
        {
            if (_registered)
                return;

            // Create a hidden window to receive hotkey messages
            var helper = new WindowInteropHelper(System.Windows.Application.Current.MainWindow);
            _windowHandle = helper.Handle;

            if (_windowHandle == IntPtr.Zero)
            {
                // If main window handle is not available, create a message-only window
                _windowHandle = CreateMessageWindow();
            }

            _source = HwndSource.FromHwnd(_windowHandle);
            _source?.AddHook(HwndHook);

            if (!RegisterHotKey(_windowHandle, HOTKEY_ID, _modifiers, _key))
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to register hotkey. Error code: {error}");
            }

            _registered = true;
        }

        /// <summary>
        /// Unregisters the hotkey
        /// </summary>
        public void Unregister()
        {
            if (!_registered)
                return;

            _source?.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            _registered = false;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                _callback?.Invoke();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private IntPtr CreateMessageWindow()
        {
            // Use the main application window handle
            var app = System.Windows.Application.Current;
            if (app?.MainWindow != null)
            {
                var helper = new WindowInteropHelper(app.MainWindow);
                if (helper.Handle == IntPtr.Zero)
                {
                    helper.EnsureHandle();
                }
                return helper.Handle;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Unregister();
        }
    }

    /// <summary>
    /// Modifier keys for hotkey combinations
    /// </summary>
    [Flags]
    public enum ModifierKeys : uint
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
