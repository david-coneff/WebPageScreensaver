using System;
using System.Diagnostics;

namespace WebPageScreensaver
{
    /// <summary>
    /// Exports and imports the app's configuration by shelling out to the Windows built-in
    /// reg.exe, rather than hand-walking the registry tree in C# -- reg.exe already correctly
    /// serializes/deserializes every value type (REG_SZ, REG_DWORD, etc.) and every subkey
    /// recursively, so reimplementing that walk here would be unneeded risk for zero benefit.
    ///
    /// Scope: this covers everything under HKEY_CURRENT_USER\Software\WebPageScreensaver -- i.e.
    /// every value read/written by <see cref="Preferences"/> and <see cref="ScreenInformation"/>:
    /// CloseOnMouseMovement, MultiScreenMode, and each DisplayN subkey's URLs, IntervalRotation,
    /// Shuffle, and ZoomPercent.
    ///
    /// OUT OF SCOPE, deliberately: login/session state. That lives separately, in the WebView2
    /// browser profile directory (%LocalAppData%\WebPageScreensaver\WebView2Profile -- cookies,
    /// localStorage, IndexedDB, etc.), not in the registry, and this class never touches it. That
    /// profile directory holds DPAPI-encrypted, per-user/per-machine credential material that
    /// would not portably survive being copied to a different Windows account or machine, and
    /// copying a live profile directory while a WebView2 environment might have it open is a
    /// separate hazard entirely. Use "Log In..." again on the new machine/account instead.
    ///
    /// Neither exporting nor importing this HKCU subtree requires administrator elevation.
    /// </summary>
    internal static class ConfigurationBackup
    {
        private const string RegistryKeyPath = @"HKCU\Software\WebPageScreensaver";

        /// <summary>
        /// Exports every value and subkey under HKCU\Software\WebPageScreensaver into a .reg file
        /// at <paramref name="filePath"/>, overwriting it if it already exists.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// reg.exe could not be started, or it exited with a non-zero exit code.
        /// </exception>
        public static void Export(string filePath)
        {
            RunRegExe($"export \"{RegistryKeyPath}\" \"{filePath}\" /y");
        }

        /// <summary>
        /// Imports the .reg file at <paramref name="filePath"/>, restoring every value and subkey
        /// it contains under HKEY_CURRENT_USER. Note that any already-open PreferencesForm will
        /// not pick up the newly-imported values on its own -- its controls were only populated
        /// once, at Load time -- so callers should tell the user to close and reopen it.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// reg.exe could not be started, or it exited with a non-zero exit code (e.g. the file was
        /// missing or was not a valid .reg file).
        /// </exception>
        public static void Import(string filePath)
        {
            RunRegExe($"import \"{filePath}\"");
        }

        /// <summary>
        /// Runs reg.exe with the given arguments, waits for it to finish, and throws with reg.exe's
        /// own error text if it fails, so a real failure is surfaced to the caller instead of being
        /// silently swallowed.
        /// </summary>
        private static void RunRegExe(string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };

            // Process.Start(ProcessStartInfo) can throw (e.g. Win32Exception if reg.exe isn't
            // found); that exception is intentionally left to propagate to the caller rather than
            // being caught and swallowed here.
            using Process? process = Process.Start(startInfo);
            if (process is null)
            {
                throw new InvalidOperationException("Unable to start reg.exe.");
            }

            // Read the redirected stream to completion before WaitForExit(): with a single
            // redirected stream (only StandardError here, not StandardOutput) this ordering is
            // safe and avoids the classic deadlock that can occur when both stdout and stderr are
            // redirected and read synchronously.
            string standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string message = string.IsNullOrWhiteSpace(standardError)
                    ? $"reg.exe exited with code {process.ExitCode}."
                    : standardError.Trim();
                throw new InvalidOperationException(message);
            }
        }
    }
}
