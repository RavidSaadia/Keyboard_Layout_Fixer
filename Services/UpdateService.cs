using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;

namespace KeyboardLayoutFixer.Services
{
    /// <summary>
    /// A newer release found on GitHub. Tag is the raw git tag (e.g. "v1.3.0"), used to build download URLs.
    /// </summary>
    public record UpdateInfo(Version Version, string Tag);

    /// <summary>
    /// Error during an update, with a message that can be shown to the user.
    /// </summary>
    public class UpdateException : Exception
    {
        public UpdateException(string message, Exception? inner = null) : base(message, inner) { }
    }

    /// <summary>
    /// Self-update from GitHub Releases.
    ///
    /// Uses the plain github.com URLs (not api.github.com) to avoid the 60 requests/hour
    /// unauthenticated API limit, which a whole office behind one NAT IP would hit.
    /// The running exe is replaced by renaming it (Windows allows renaming, not deleting,
    /// a running exe), so no helper script is needed.
    /// </summary>
    public class UpdateService
    {
        public const string RepoUrl = "https://github.com/RavidSaadia/Keyboard_Layout_Fixer";
        public const string LatestReleaseUrl = RepoUrl + "/releases/latest";
        public const string AssetName = "KeyboardLayoutFixer.exe";

        private readonly Action<string> _log;
        private readonly string _userAgent;

        public UpdateService(string currentVersion, Action<string> log)
        {
            _log = log;
            _userAgent = $"KeyboardLayoutFixer/{currentVersion}";
        }

        /// <summary>
        /// True only when running as a published single-file exe on Windows.
        /// Dev builds (dotnet run, bin\) have KeyboardLayoutFixer.dll next to the exe and must not replace themselves.
        /// </summary>
        public static bool CanSelfUpdate =>
            OperatingSystem.IsWindows() &&
            Environment.ProcessPath != null &&
            !File.Exists(Path.Combine(AppContext.BaseDirectory, "KeyboardLayoutFixer.dll"));

        /// <summary>
        /// Extracts the version from a release URL like ".../releases/tag/v1.3.0". Returns null if not a plain version tag.
        /// </summary>
        public static Version? ParseTagFromLocation(string location)
        {
            string? tag = ExtractTag(location);
            if (tag == null) return null;

            string versionText = tag.TrimStart('v', 'V');
            return Version.TryParse(versionText, out var version) ? version : null;
        }

        /// <summary>
        /// True if remote is strictly newer than current. Missing components count as 0, so 1.3.0 == 1.3.0.0.
        /// </summary>
        public static bool IsNewer(Version remote, Version current) => Normalize(remote) > Normalize(current);

        /// <summary>
        /// Asks GitHub which release is latest (via the /releases/latest redirect). Returns null if none or on error.
        /// </summary>
        public async Task<UpdateInfo?> GetLatestReleaseAsync()
        {
            try
            {
                using var handler = new HttpClientHandler { AllowAutoRedirect = false };
                using var client = CreateClient(handler, TimeSpan.FromSeconds(30));
                using var response = await client.GetAsync(LatestReleaseUrl, HttpCompletionOption.ResponseHeadersRead);

                string? location = response.Headers.Location?.ToString();
                if (location == null)
                {
                    _log($"Update check: no redirect from {LatestReleaseUrl} (HTTP {(int)response.StatusCode}); no releases yet?");
                    return null;
                }

                string? tag = ExtractTag(location);
                Version? version = ParseTagFromLocation(location);
                if (tag == null || version == null)
                {
                    _log($"Update check: could not parse version from '{location}'");
                    return null;
                }

                return new UpdateInfo(version, tag);
            }
            catch (Exception ex)
            {
                _log($"Update check failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Downloads the release exe, verifies its SHA-256, swaps it in place of the running exe
        /// and starts it. The caller must shut down the current process afterwards.
        /// On failure the original exe is left in place and an UpdateException is thrown.
        /// </summary>
        public async Task InstallAsync(UpdateInfo update)
        {
            string exePath = Environment.ProcessPath
                ?? throw new UpdateException("Could not determine the application's location.");
            string newPath = exePath + ".new";
            string oldPath = exePath + ".old";
            string downloadBase = $"{RepoUrl}/releases/download/{update.Tag}/";

            try
            {
                using var client = CreateClient(new HttpClientHandler(), TimeSpan.FromMinutes(10));

                // 1. Download the exe next to the current one (same folder => renames are atomic)
                _log($"Downloading {downloadBase}{AssetName}");
                await using (var source = await client.GetStreamAsync(downloadBase + AssetName))
                await using (var target = File.Create(newPath))
                {
                    await source.CopyToAsync(target);
                }

                // 2. Verify checksum
                string expected = (await client.GetStringAsync(downloadBase + AssetName + ".sha256"))
                    .Trim().Split(' ', '\t', '\r', '\n')[0];
                string actual;
                await using (var stream = File.OpenRead(newPath))
                {
                    actual = Convert.ToHexString(await SHA256.HashDataAsync(stream));
                }

                if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                    throw new UpdateException("The downloaded file is corrupted (checksum mismatch).");

                _log("Checksum verified");
            }
            catch (Exception ex)
            {
                TryDelete(newPath);
                if (ex is UpdateException) throw;
                if (ex is UnauthorizedAccessException)
                    throw new UpdateException("No permission to write to the application's folder.", ex);
                throw new UpdateException($"Download failed: {ex.Message}", ex);
            }

            // 3. Swap: rename running exe away, move new one into place; roll back on failure
            try
            {
                TryDelete(oldPath);
                File.Move(exePath, oldPath);
                try
                {
                    File.Move(newPath, exePath);
                }
                catch
                {
                    File.Move(oldPath, exePath);
                    throw;
                }
            }
            catch (Exception ex)
            {
                TryDelete(newPath);
                throw new UpdateException($"Could not replace the application file: {ex.Message}", ex);
            }

            _log($"Installed {update.Tag}, starting new version");

            // 4. Start the new version (its startup also closes this process if it lingers)
            Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
        }

        /// <summary>
        /// Deletes files left over from a previous update (.old from the swap, .new from an interrupted download).
        /// </summary>
        public static void CleanupLeftovers()
        {
            string? exePath = Environment.ProcessPath;
            if (exePath == null) return;
            TryDelete(exePath + ".old");
            TryDelete(exePath + ".new");
        }

        private HttpClient CreateClient(HttpMessageHandler handler, TimeSpan timeout)
        {
            var client = new HttpClient(handler, disposeHandler: true) { Timeout = timeout };
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
            return client;
        }

        private static string? ExtractTag(string location)
        {
            const string marker = "/tag/";
            int index = location.LastIndexOf(marker, StringComparison.Ordinal);
            if (index < 0) return null;

            string tag = location[(index + marker.Length)..].TrimEnd('/');
            return tag.Length > 0 ? Uri.UnescapeDataString(tag) : null;
        }

        private static Version Normalize(Version v) =>
            new(v.Major, v.Minor, Math.Max(v.Build, 0), Math.Max(v.Revision, 0));

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch { }
        }
    }
}
