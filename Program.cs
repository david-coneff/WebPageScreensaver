using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace WebPageScreensaver
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (Process.GetCurrentProcess().MainModule is not ProcessModule)
            {
                throw new NullReferenceException("Current process main module is null.");
            }

            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2); // Helps respect the specified window sizes
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true); // Prevents seeing tiny unexpected fonts

            // Argument verification:
            // - The arguments /C, /P and /S are required by Windows Control Panel.
            // - Windows' own "Screen Saver Settings" dialog invokes the Settings... button with
            //   a parent window handle attached to the verb -- historically as "/c:1234567" (one
            //   token) or as two separate tokens ("/c" "1234567") depending on the Windows
            //   version/invocation path. This app does not embed itself as a child of that handle
            //   (that would need Win32 SetParent/WS_CHILD interop, out of scope here), but it must
            //   still recognize the verb -- an exact whole-argument match on "/C" alone silently
            //   missed both real forms, which is the documented "Settings button does nothing"
            //   issue in this project's README. Matching by PREFIX on args[0] and ignoring any
            //   handle that follows (attached or a separate second argument) fixes that.

            // Passing no arguments is interpreted as using "/C"
            if (args.Length == 0)
            {
                ShowPreferences();
                return;
            }

            string verb = args[0];
            if (verb.StartsWith("/C", StringComparison.OrdinalIgnoreCase)) // Configure
            {
                ShowPreferences();
            }
            else if (verb.StartsWith("/P", StringComparison.OrdinalIgnoreCase) // Preview
                || verb.StartsWith("/S", StringComparison.OrdinalIgnoreCase)) // Show
            {
                ShowScreenSaver();
            }
            else
            {
                Console.WriteLine($"Unrecognized argument: {verb}");
            }
        }

        /// <summary>
        /// Show the screensaver preferences window.
        /// </summary>
        private static void ShowPreferences()
        {
            Application.Run(new PreferencesForm());
        }

        /// <summary>
        /// Shows the screensaver form in all the screens.
        /// </summary>
        private static void ShowScreenSaver()
        {
            // Scoped to the whole /s process, not per-monitor form: one disable/restore pair
            // covers every ScreensaverForm regardless of how many screens are attached.
            AccessibilityShortcuts.Disable();
            try
            {
                var forms = new List<Form>();

                foreach ((int _, ScreenInformation info) in Preferences.Screens)
                {
                    var form = new ScreensaverForm(info);
                    forms.Add(form);
                }

                Application.Run(new MultiFormContext(forms));
            }
            finally
            {
                AccessibilityShortcuts.Restore();
            }
        }
    }
}
