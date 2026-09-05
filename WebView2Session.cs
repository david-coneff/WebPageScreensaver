using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace WebPageScreensaver
{
    /// <summary>
    /// The one thing that makes "log in once, stay logged in as the screensaver" work: every
    /// WebView2 control in this app — the screensaver's own display AND the setup/login window —
    /// must share the SAME on-disk user data folder (UDF), because that folder is where WebView2
    /// keeps cookies, localStorage, IndexedDB and cached credentials. Two controls pointed at two
    /// DIFFERENT folders are, as far as the target website is concerned, two different browsers
    /// that have never met; navigating the screensaver's own WebView2 to a URL a user logged into
    /// during setup would show a fresh, logged-out page.
    ///
    /// This does NOT store any credentials itself. It relies entirely on WebView2's own
    /// (Chromium's own) persistent-profile mechanism — the same one Edge/Chrome use for every
    /// ordinary logged-in browsing session, DPAPI-encrypted per Windows user. There is no custom
    /// credential vault to secure, audit, or get wrong.
    ///
    /// The folder is placed under %LocalAppData%, not next to the executable, because WebView2's
    /// OWN default (when no environment is created explicitly) tries to create the UDF beside the
    /// running exe — which fails silently into an unpredictable fallback once this screensaver is
    /// installed to C:\Windows\System32\ (a protected directory an ordinary user cannot write to).
    /// An explicit, always-writable, per-user path removes that ambiguity entirely, independent of
    /// where the .scr happens to be installed.
    /// </summary>
    internal static class WebView2Session
    {
        private static readonly string UserDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WebPageScreensaver", "WebView2Profile");

        private static Task<CoreWebView2Environment>? _environmentTask;

        /// <summary>
        /// The shared environment for this PROCESS. Safe to call from multiple forms in the same
        /// process (the screensaver creates one per monitor) — WebView2Environment is designed to
        /// be reused across controls, and this caches the one creation call.
        ///
        /// NOT safe across PROCESSES running concurrently: WebView2 takes an exclusive lock on the
        /// user data folder for as long as any environment built from it is alive, so a second,
        /// separate process (e.g. the actual screensaver kicking in while the login window from
        /// "/c" is still open) will fail here rather than corrupt shared state. Callers must treat
        /// that as an ordinary, expected failure — see <see cref="TryGetEnvironmentAsync"/> — not
        /// a bug: in practice Windows will not normally start the screensaver while another window
        /// belonging to the same desktop session is actively in use, but a long-idle login window
        /// left open is exactly the case where it could.
        /// </summary>
        public static Task<CoreWebView2Environment> GetEnvironmentAsync()
        {
            // Not locked: worst case under a rare race is one extra CreateAsync call whose result
            // is discarded, which is harmless — CoreWebView2Environment.CreateAsync is idempotent
            // for a given folder, just wasteful to call twice. A lock would protect nothing real.
            _environmentTask ??= CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null, userDataFolder: UserDataFolder, options: null);
            return _environmentTask;
        }

        /// <summary>
        /// <see cref="GetEnvironmentAsync"/>, but reports failure instead of letting the
        /// exception propagate — the expected failure mode (UDF locked by another live process
        /// using this same profile) is routine enough that every caller needs to handle it, not
        /// crash on it.
        /// </summary>
        public static async Task<(CoreWebView2Environment? Environment, Exception? Error)> TryGetEnvironmentAsync()
        {
            try
            {
                return (await GetEnvironmentAsync(), null);
            }
            catch (Exception ex)
            {
                // Reset so a LATER call (once whatever was holding the lock has closed) gets a
                // fresh attempt instead of replaying this same failed Task forever.
                _environmentTask = null;
                return (null, ex);
            }
        }
    }
}
