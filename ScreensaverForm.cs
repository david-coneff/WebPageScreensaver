using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WebPageScreensaver
{
    internal partial class ScreensaverForm : Form
    {
        private const int HOTKEY_ID = 0xBEEF;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private int _currentURLIndex;

        private readonly Timer _timer;
        // Timer that hides the cursor after a period of inactivity
        private readonly Timer _idleTimer;
        private const int IdleTimeoutMs = 5 * 60 * 1000; // 5 minutes
        private InputActivityMessageFilter _activityFilter;
        private readonly bool _closeOnMouseMovement;
        private readonly int _rotationInterval;
        private readonly bool _shuffle;
        private readonly List<string> _urls;
        private readonly double _zoomFactor;
        private readonly HashSet<string> _allowedHosts;
        private readonly Size _savedSize;
        private readonly Point _savedLocation;

        public ScreensaverForm(ScreenInformation screen)
        {
            _currentURLIndex = 0;

            _closeOnMouseMovement = Preferences.CloseOnMouseMovement;
            _rotationInterval = screen.RotationInterval;
            _shuffle = screen.Shuffle;
            _urls = screen.URLs.ToList();
            _zoomFactor = screen.ZoomPercent / 100.0;
            _allowedHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string url in _urls)
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri? parsed))
                {
                    _allowedHosts.Add(parsed.Host);
                }
            }

            _savedSize = new Size(screen.Bounds.Width, screen.Bounds.Height);
            _savedLocation = new Point(screen.Bounds.Left, screen.Bounds.Top);

            InitializeComponent();

            // Let the form receive key events before the focused control (helps capture Escape)
            this.KeyPreview = true;
            // Ensure Load handler is attached
            this.Load += ScreensaverForm_Load;

            // Manually change size and location, since the `InitializeComponent` code tends to get autoreplaced by the Designer
            this.SuspendLayout();
            this._webBrowser.Size = _savedSize;
            this._webBrowser.Location = _savedLocation;
            this.ClientSize = _savedSize;
            this.Location = _savedLocation;
            this.ResumeLayout(false);

            _timer = new Timer();

            // Idle timer: hide cursor after IdleTimeoutMs of no user activity
            _idleTimer = new Timer();
            _idleTimer.Interval = IdleTimeoutMs;
            _idleTimer.Tick += IdleTimer_Tick;

            // Install a message filter that notifies on any user input so we can reset the idle timer
            _activityFilter = new InputActivityMessageFilter();
            _activityFilter.UserActivity += ActivityFilter_UserActivity;
            Application.AddMessageFilter(_activityFilter);
            // Try to catch ESC when the WebView2 control forwards preview key events
            _webBrowser.PreviewKeyDown += WebBrowser_PreviewKeyDown;
            _idleTimer.Start();
        }

        private void IdleTimer_Tick(object? sender, EventArgs e)
        {
            // Hide the cursor when we've been idle for the configured timeout
            Cursor.Hide();
            _idleTimer.Stop();
        }

        private void ActivityFilter_UserActivity(object? sender, EventArgs e)
        {
            // Any user activity should make the cursor visible and restart the idle timer
            Cursor.Show();

            // If configured to close on mouse movement, close immediately on mouse activity
            if (_closeOnMouseMovement)
            {
                Close();
                return;
            }

            // Restart the idle timer so the cursor will be hidden again after the timeout
            if (!_idleTimer.Enabled)
            {
                _idleTimer.Start();
            }
            else
            {
                _idleTimer.Stop();
                _idleTimer.Start();
            }
        }

        private async void ScreensaverForm_Load(object sender, EventArgs e)
        {
            if (_webBrowser == null)
            {
                throw new NullReferenceException("webBrowser should have been initialized by now.");
            }

            // Shared with the setup/login window (LoginForm) — see WebView2Session for why this
            // is what makes the screensaver display an already-logged-in page rather than a
            // fresh login form.
            (Microsoft.Web.WebView2.Core.CoreWebView2Environment? environment, Exception? error) =
                await WebView2Session.TryGetEnvironmentAsync();
            if (environment == null)
            {
                // Rare (see WebView2Session's docstring): degrade to a blank screen instead of
                // crashing the whole screensaver over what is normally a transient lock.
                Debug.WriteLine($"WebPageScreensaver: could not initialize the shared browser session: {error}");
                _webBrowser.Visible = false;
                return;
            }

            await _webBrowser.EnsureCoreWebView2Async(environment);

            HardenForUnattendedDisplay(_webBrowser.CoreWebView2);

            // Applied here for the FIRST navigation, and again in NavigationCompleted below:
            // Chromium remembers a per-origin zoom level in the profile itself, which — now that
            // this app shares one persistent profile across every launch (WebView2Session) — can
            // silently override a bare one-time assignment the next time a page from that origin
            // loads. Re-asserting it after every navigation is what makes the configured default
            // actually stick rather than just applying "most of the time".
            _webBrowser.ZoomFactor = _zoomFactor;
            _webBrowser.CoreWebView2.NavigationCompleted += (s, e) => _webBrowser.ZoomFactor = _zoomFactor;

            // Note: do not use CoreWebView2.AcceleratorKeyPressed (may not be available in this SDK).
            // We rely on form KeyPreview, PreviewKeyDown on the WebView and the IMessageFilter fallback.

            // Inject a script into every page loaded in the WebView2 that posts a message
            // to the host when the Escape key is pressed. This is the most reliable way
            // to observe plain Escape presses when the page has focus.
            try
            {
                await _webBrowser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
                    "document.addEventListener('keydown', function(e) { if (e.key === 'Escape') { window.chrome.webview.postMessage('escape'); } }, true);");

                _webBrowser.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            }
            catch
            {
                // Ignore failures to inject script on older runtimes; other fallbacks remain.
            }

            // Register a system hotkey for plain Escape as a reliable fallback when WebView2
            // consumes keyboard input. We register it with no modifiers.
            RegisterHotKey(this.Handle, HOTKEY_ID, 0, (uint)Keys.Escape);

            if (_urls.Any())
            {
                if (_shuffle)
                {
                    Random random = new Random();
                    int n = _urls.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = random.Next(n + 1);
                        var value = _urls[k];
                        _urls[k] = _urls[n];
                        _urls[n] = value;
                    }
                }

                _timer.Interval = _rotationInterval * 1000;
                _timer.Tick += (s, ee) => RotateSite();
                _timer.Start();

                RotateSite(); // First call, second one will be done _rotationInterval seconds later by _timer
            }
            else
            {
                _webBrowser.Visible = false;
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                // our registered hotkey id
                if (id == 0xBEEF)
                {
                    Application.Exit();
                    return;
                }
            }

            base.WndProc(ref m);
        }

        // AcceleratorKeyPressed handler removed: not available in all WebView2 SDKs.

        private void CoreWebView2_WebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var msg = e.TryGetWebMessageAsString();
                if (string.Equals(msg, "escape", StringComparison.OrdinalIgnoreCase))
                {
                    Application.Exit();
                }
            }
            catch { }
        }

        private void WebBrowser_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            try
            {
                // Ensure timers are stopped and message filter removed
                _timer?.Stop();
                _idleTimer?.Stop();
                if (_activityFilter != null)
                {
                    Application.RemoveMessageFilter(_activityFilter);
                    _activityFilter.UserActivity -= ActivityFilter_UserActivity;
                }

                // No CoreWebView2 accelerator to unsubscribe; nothing to do here.

                // Unregister hotkey
                UnregisterHotKey(this.Handle, HOTKEY_ID);
                // Unsubscribe WebMessageReceived if we subscribed
                try
                {
                    if (_webBrowser?.CoreWebView2 != null)
                    {
                        _webBrowser.CoreWebView2.WebMessageReceived -= CoreWebView2_WebMessageReceived;
                    }
                }
                catch { }
            }
            finally
            {
                // Make sure cursor is visible when exiting
                Cursor.Show();
            }
        }

        private void RotateSite()
        {
            if (_currentURLIndex >= _urls.Count)
            {
                _currentURLIndex = 0;
            }
            BrowseTo(_urls[_currentURLIndex]);
            _currentURLIndex++;
        }

        private void BrowseTo(string url)
        {
            _webBrowser.Visible = true;
            _webBrowser.CoreWebView2.Navigate(url);
        }

        /// <summary>
        /// Restricts what an unattended, non-interactive display can be made to do. A screensaver
        /// is shown to whoever is physically at the machine, logged-in user or not, and Chromium's
        /// default browser surface is much larger than "render a page": disabled here are all the
        /// paths that would otherwise pop a native, Explorer-shell-backed dialog (Save As, Print's
        /// "Microsoft Print to PDF" file picker, an Open File picker) or hand a passerby a second,
        /// uncontrolled window or a JS console. None of these restrictions apply to LoginForm —
        /// reaching that window already requires an authenticated, unlocked desktop, so it grants
        /// no new privilege a normal browser wouldn't.
        /// </summary>
        private void HardenForUnattendedDisplay(Microsoft.Web.WebView2.Core.CoreWebView2 core)
        {
            core.Settings.AreDefaultContextMenusEnabled = false;   // no "Save image as", "Inspect", "View source"
            core.Settings.AreDevToolsEnabled = false;              // no F12 / Ctrl+Shift+I console
            core.Settings.AreBrowserAcceleratorKeysEnabled = false; // no Ctrl+P, Ctrl+S, Ctrl+O, Ctrl+F, F5, F12, ...
            core.Settings.IsZoomControlEnabled = false;            // the configured zoom is authoritative; no accidental drift

            // Any window.open() / target="_blank" is redirected into THIS window instead of
            // spawning a second, unrestricted one — a fresh popup would not inherit the settings
            // above, and there is no legitimate reason for a second visible window to exist here.
            core.NewWindowRequested += (s, e) =>
            {
                e.Handled = true;
                if (!string.IsNullOrEmpty(e.Uri))
                {
                    core.Navigate(e.Uri);
                }
            };

            // A screensaver never legitimately needs to write a file to disk.
            core.DownloadStarting += (s, e) => e.Cancel = true;

            // Keep the TOP-LEVEL page on one of the configured screen's own hosts (or a subdomain
            // of one). This is deliberately approximate — it allows any subdomain of a configured
            // host rather than resolving true registrable domains (e.g. it would also allow
            // "evil.co.uk" if "example.co.uk" were configured), which would need a public-suffix
            // list this app doesn't carry — but it closes the main risk (a redirect or a clicked
            // link taking the unattended display to attacker-controlled content entirely) without
            // an external dependency. It does not apply to sub-resources or iframes within an
            // allowed page, only to navigation of the page itself.
            core.NavigationStarting += (s, e) =>
            {
                if (e.Uri == "about:blank" || !Uri.TryCreate(e.Uri, UriKind.Absolute, out Uri? target))
                {
                    return;
                }

                bool allowed = _allowedHosts.Any(host =>
                    string.Equals(target.Host, host, StringComparison.OrdinalIgnoreCase) ||
                    target.Host.EndsWith("." + host, StringComparison.OrdinalIgnoreCase));

                if (!allowed)
                {
                    e.Cancel = true;
                    Debug.WriteLine($"WebPageScreensaver: blocked navigation to disallowed host '{target.Host}'");
                }
            };
        }

        private void WebBrowser_MouseMove(object sender, EventArgs e)
        {
            if (_closeOnMouseMovement)
            {
                Close();
            }
        }

        /// <summary>
        /// Allows capturing the ESC key to close the form.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Exit on Escape
            if ((keyData & Keys.KeyCode) == Keys.Escape)
            {
                Application.Exit();
                return true;
            }

            // Allow Alt+F4 to fall through to default handling (closes the window)
            if ((keyData & Keys.KeyCode) == Keys.F4 && (keyData & Keys.Alt) == Keys.Alt)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            // NOTE: Secure attention sequences like Ctrl+Alt+Delete cannot be intercepted
            // by user-mode applications — the OS handles them and they cannot be forwarded/blocked here.

            // Consume every other key so it does nothing
            return true;
        }
    }

    /// <summary>
    /// Watches for basic user input (mouse move, mouse clicks, key presses) and exposes a UserActivity event.
    /// </summary>
    internal class InputActivityMessageFilter : IMessageFilter
    {
        public event EventHandler? UserActivity;

        // Window message constants we care about
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_KEYDOWN = 0x0100;

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOUSEMOVE:
                case WM_LBUTTONDOWN:
                case WM_RBUTTONDOWN:
                case WM_MBUTTONDOWN:
                case WM_MOUSEWHEEL:
                    OnUserActivity();
                    break;
                case WM_KEYDOWN:
                    // If Escape pressed at the application level, exit immediately.
                    int vk = (int)(m.WParam.ToInt64() & 0xFFFF);
                    if (vk == 0x1B) // VK_ESCAPE
                    {
                        Application.Exit();
                        return true; // consumed
                    }
                    OnUserActivity();
                    break;
            }

            // Do not block the message from reaching controls
            return false;
        }

        private void OnUserActivity()
        {
            UserActivity?.Invoke(this, EventArgs.Empty);
        }
    }
}