namespace KeyboardLayoutFixer.Platform
{
    /// <summary>
    /// Factory for creating platform-specific service implementations.
    /// </summary>
    public static class PlatformServiceFactory
    {
        public static IPlatformServices Create()
        {
            if (OperatingSystem.IsWindows())
                return new Windows.WindowsPlatformServices();

            if (OperatingSystem.IsMacOS())
                return new MacOS.MacOSPlatformServices();

            throw new PlatformNotSupportedException(
                "Keyboard Layout Fixer currently supports Windows and macOS only.");
        }
    }
}
