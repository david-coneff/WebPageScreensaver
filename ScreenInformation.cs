using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WebPageScreensaver
{
    internal class ScreenInformation
    {
        private const string URLsName = "URLs";
        // Written by versions that rotated through several URLs; only deleted now.
        private const string RotationIntervalName = "IntervalRotation";
        private const string ShuffleName = "Shuffle";
        private const string ZoomPercentName = "ZoomPercent";

        public ScreenInformation(RegistryKey rootKey, int screenNumber, MultiScreenMode multiScreenMode)
        {
            RootKey = rootKey;
            Bounds = multiScreenMode switch
            {
                MultiScreenMode.Span => FindEnclosingRectangle(),
                MultiScreenMode.Mirror or MultiScreenMode.Separate => Screen.AllScreens[screenNumber].Bounds,
                _ => throw new IndexOutOfRangeException("Unrecognized MultiScreenMode value.")
            };
        }

        public RegistryKey RootKey { get; private set; }

        public Rectangle Bounds { get; set; }

        /// <summary>
        /// The one web page this screen shows. Each screen used to hold a space-separated list of
        /// URLs that the screensaver rotated through (optionally shuffled); rotation was removed
        /// (WebPageScreensaver-memory:pando-0007), and the value keeps its registry name "URLs"
        /// so settings and backup files written before then still load: only their first entry is
        /// read. Saving writes the single URL and deletes the rotation values nothing reads now.
        /// </summary>
        public string URL
        {
            get => FirstURL(RootKey.GetOrCreateValue(URLsName, "https://dotnet.microsoft.com/" /* default */));
            set
            {
                RootKey.SetValue(URLsName, FirstURL(value));
                RootKey.DeleteValue(RotationIntervalName, throwOnMissingValue: false);
                RootKey.DeleteValue(ShuffleName, throwOnMissingValue: false);
            }
        }

        /// <summary>The first entry of a stored "URLs" value, which before rotation was removed could be a space-separated list.</summary>
        internal static string FirstURL(string stored) =>
            stored.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? string.Empty;

        /// <summary>
        /// The WebView2 zoom level to apply to every page shown on this screen, as a percentage
        /// (100 = no zoom). Lets a display that needs a dashboard scaled up or down — e.g. a
        /// Home Assistant panel sized for a different resolution than it was designed for — default
        /// to the right zoom on every screensaver launch, instead of it resetting each time.
        /// </summary>
        public int ZoomPercent
        {
            get => int.Parse(RootKey.GetOrCreateValue(ZoomPercentName, 100 /* default */));
            set => RootKey.SetValue(ZoomPercentName, value);
        }

        private static Rectangle FindEnclosingRectangle()
        {
            IEnumerable<Rectangle> bounds = Screen.AllScreens.Select(r => r.Bounds);
            return Rectangle.FromLTRB(
                bounds.Min(r => r.Left),
                bounds.Min(r => r.Top),
                bounds.Max(r => r.Right),
                bounds.Max(r => r.Bottom));
        }
    }
}
