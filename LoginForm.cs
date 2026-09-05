using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WebPageScreensaver
{
    /// <summary>
    /// The "log in, then close this window" step of setup. Hosts a normal, resizable WebView2
    /// browser pointed at the SAME shared, persistent profile the screensaver itself uses (see
    /// <see cref="WebView2Session"/>) — so whatever the user logs into here is what the
    /// screensaver later displays already logged in. There is no explicit "save" action:
    /// WebView2 persists cookies and storage to disk continuously as the user browses, not just
    /// on close — closing this window is the whole "setup" step.
    /// </summary>
    internal partial class LoginForm : Form
    {
        public LoginForm(string initialUrl)
        {
            InitializeComponent();
            _textBoxAddress.Text = initialUrl;
        }

        private async void LoginForm_Load(object sender, EventArgs e)
        {
            (Microsoft.Web.WebView2.Core.CoreWebView2Environment? environment, Exception? error) =
                await WebView2Session.TryGetEnvironmentAsync();
            if (environment == null)
            {
                MessageBox.Show(this,
                    "Could not open the browser session. If the screensaver preview or another " +
                    "login window is already open, close it first and try again." +
                    Environment.NewLine + Environment.NewLine + error,
                    "Web Page Screensaver", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            await _webView.EnsureCoreWebView2Async(environment);
            // Keep the address bar honest as the user navigates (links, redirects, OAuth
            // hand-offs to a different domain) — worth having in a window whose entire purpose
            // is typing credentials into whatever page is currently shown.
            _webView.CoreWebView2.SourceChanged += (sourceSender, sourceArgs) =>
                _textBoxAddress.Text = _webView.Source?.ToString() ?? string.Empty;
            Navigate();
        }

        private void ButtonGo_Click(object sender, EventArgs e) => Navigate();

        private void TextBoxAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Navigate();
            }
        }

        private void Navigate()
        {
            string url = _textBoxAddress.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            if (!url.Contains("://", StringComparison.Ordinal))
            {
                url = "https://" + url;
            }

            try
            {
                _webView.CoreWebView2?.Navigate(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WebPageScreensaver: navigation to '{url}' failed: {ex}");
            }
        }
    }
}
