using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WebPageScreensaver
{
    /// <summary>
    /// The "/p &lt;hwnd&gt;" preview: a static placeholder drawn INSIDE the little monitor
    /// picture of Windows' Screen Saver Settings dialog, whose window handle Windows passes.
    /// The form is created as a WS_CHILD of that window (CreateParams.Parent), sized to its
    /// client area, and closes when the dialog destroys it. It shows the product name and
    /// the first configured URL rather than a live page: a WebView2 here would start a
    /// browser process every time the dialog opens or the selection changes.
    /// </summary>
    internal sealed class PreviewForm : Form
    {
        private const int WS_CHILD = 0x40000000;
        private const int WS_VISIBLE = 0x10000000;
        private const int WS_CLIPSIBLINGS = 0x04000000;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        private readonly IntPtr _parent;
        private readonly string _caption;
        private readonly Timer _parentWatch = new Timer { Interval = 500 };

        /// <summary>True when <paramref name="hwnd"/> names a live window to draw into.</summary>
        internal static bool IsUsableParent(IntPtr hwnd) => hwnd != IntPtr.Zero && IsWindow(hwnd);

        internal PreviewForm(IntPtr parent)
        {
            _parent = parent;
            _caption = FirstUrl();
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.Black;
            DoubleBuffered = true;
            if (GetClientRect(parent, out RECT r))
            {
                Bounds = new Rectangle(0, 0, r.Right - r.Left, r.Bottom - r.Top);
            }
            // The dialog destroys the preview window when it closes or another saver is
            // picked; a child form is not told, so poll for it and exit.
            _parentWatch.Tick += (_, _) => { if (!IsWindow(_parent)) Close(); };
            _parentWatch.Start();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Parent = _parent;
                cp.Style = WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS;
                cp.ExStyle = 0;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle area = ClientRectangle;
            using var title = new Font(FontFamily.GenericSansSerif, Math.Max(6f, area.Height / 9f), FontStyle.Bold, GraphicsUnit.Pixel);
            using var small = new Font(FontFamily.GenericSansSerif, Math.Max(5f, area.Height / 14f), GraphicsUnit.Pixel);
            var centre = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
            var top = new Rectangle(area.X + 4, area.Y, area.Width - 8, area.Height / 2);
            var bottom = new Rectangle(area.X + 4, area.Y + area.Height / 2, area.Width - 8, area.Height / 2);
            e.Graphics.DrawString("Web Page Screensaver", title, Brushes.White, top, centre);
            e.Graphics.DrawString(_caption, small, Brushes.Gray, bottom, centre);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _parentWatch.Dispose();
            }
            base.Dispose(disposing);
        }

        private static string FirstUrl()
        {
            try
            {
                return Preferences.Screens.Values.SelectMany(s => s.URLs).FirstOrDefault(u => !string.IsNullOrWhiteSpace(u)) ?? "(no URL set)";
            }
            catch (Exception)
            {
                return "";   // a preview must never fail the dialog over a preferences read
            }
        }
    }
}
