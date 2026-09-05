using System.Runtime.InteropServices;

namespace WebPageScreensaver
{
    /// <summary>
    /// Temporarily disables the keyboard shortcuts that ACTIVATE Windows' Sticky Keys, Toggle
    /// Keys, and Filter Keys accessibility features (5x Shift, holding Right-Shift for 8s,
    /// holding NumLock for 5s) while the screensaver is displayed, restoring the exact prior
    /// settings afterward.
    ///
    /// This closes a well-documented, real-world kiosk/lock-screen bypass: all three shortcuts
    /// are handled by a system accessibility component OUTSIDE any application's window, so they
    /// fire even over a full-screen topmost app — the same mechanism behind the classic
    /// "replace sethc.exe" Windows login-bypass technique — and their dialog links into Control
    /// Panel / Settings. Disabling ACTIVATION here does not touch the underlying accessibility
    /// features themselves: a user who already has Sticky Keys turned on in Settings keeps it;
    /// this only suppresses the shortcut that turns it on from an idle/off state, which has no
    /// legitimate purpose while a screensaver is showing.
    ///
    /// Deliberately does NOT attempt to block the Windows key, Alt+Tab, or Ctrl+Shift+Esc (Task
    /// Manager) — those are shell-level shortcuts that would need a global low-level keyboard
    /// hook (SetWindowsHookEx/WH_KEYBOARD_LL) to suppress, a materially bigger and riskier
    /// change than a few accessibility-preference flags, and one that cannot be verified in an
    /// environment that cannot run this app at all. And it cannot and does not touch Ctrl+Alt+Del
    /// — the Secure Attention Sequence is a hard OS guarantee no user-mode process can intercept,
    /// by design.
    /// </summary>
    internal static class AccessibilityShortcuts
    {
        private const uint SPI_GETSTICKYKEYS = 0x003A;
        private const uint SPI_SETSTICKYKEYS = 0x003B;
        private const uint SPI_GETTOGGLEKEYS = 0x0034;
        private const uint SPI_SETTOGGLEKEYS = 0x0035;
        private const uint SPI_GETFILTERKEYS = 0x0032;
        private const uint SPI_SETFILTERKEYS = 0x0033;

        // Shared bit positions across all three structs' dwFlags.
        private const uint HOTKEYACTIVE = 0x4000;
        private const uint CONFIRMHOTKEY = 0x8000;

        [StructLayout(LayoutKind.Sequential)]
        private struct STICKYKEYS
        {
            public uint cbSize;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOGGLEKEYS
        {
            public uint cbSize;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FILTERKEYS
        {
            public uint cbSize;
            public uint dwFlags;
            public uint iWaitMSec;
            public uint iDelayMSec;
            public uint iRepeatMSec;
            public uint iBounceMSec;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref STICKYKEYS pvParam, uint fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref TOGGLEKEYS pvParam, uint fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref FILTERKEYS pvParam, uint fWinIni);

        private static STICKYKEYS _originalSticky;
        private static TOGGLEKEYS _originalToggle;
        private static FILTERKEYS _originalFilter;
        private static bool _disabled;

        /// <summary>Best-effort; never throws — a failure here must not stop the screensaver
        /// from displaying. Idempotent: a second call while already disabled does nothing.</summary>
        public static void Disable()
        {
            if (_disabled)
            {
                return;
            }

            try
            {
                _originalSticky = new STICKYKEYS { cbSize = (uint)Marshal.SizeOf<STICKYKEYS>() };
                SystemParametersInfo(SPI_GETSTICKYKEYS, 0, ref _originalSticky, 0);
                STICKYKEYS sticky = _originalSticky;
                sticky.dwFlags &= ~(HOTKEYACTIVE | CONFIRMHOTKEY);
                SystemParametersInfo(SPI_SETSTICKYKEYS, (uint)Marshal.SizeOf<STICKYKEYS>(), ref sticky, 0);

                _originalToggle = new TOGGLEKEYS { cbSize = (uint)Marshal.SizeOf<TOGGLEKEYS>() };
                SystemParametersInfo(SPI_GETTOGGLEKEYS, 0, ref _originalToggle, 0);
                TOGGLEKEYS toggle = _originalToggle;
                toggle.dwFlags &= ~(HOTKEYACTIVE | CONFIRMHOTKEY);
                SystemParametersInfo(SPI_SETTOGGLEKEYS, (uint)Marshal.SizeOf<TOGGLEKEYS>(), ref toggle, 0);

                _originalFilter = new FILTERKEYS { cbSize = (uint)Marshal.SizeOf<FILTERKEYS>() };
                SystemParametersInfo(SPI_GETFILTERKEYS, 0, ref _originalFilter, 0);
                FILTERKEYS filter = _originalFilter;
                filter.dwFlags &= ~(HOTKEYACTIVE | CONFIRMHOTKEY);
                SystemParametersInfo(SPI_SETFILTERKEYS, (uint)Marshal.SizeOf<FILTERKEYS>(), ref filter, 0);

                _disabled = true;
            }
            catch
            {
                // Never let an accessibility-lockdown failure take down the screensaver.
            }
        }

        /// <summary>Restores exactly the settings <see cref="Disable"/> found — never a
        /// hardcoded "turn it back on", since the original state might already have had these
        /// shortcuts off, or Sticky Keys itself genuinely enabled by the user.</summary>
        public static void Restore()
        {
            if (!_disabled)
            {
                return;
            }

            try
            {
                SystemParametersInfo(SPI_SETSTICKYKEYS, (uint)Marshal.SizeOf<STICKYKEYS>(), ref _originalSticky, 0);
                SystemParametersInfo(SPI_SETTOGGLEKEYS, (uint)Marshal.SizeOf<TOGGLEKEYS>(), ref _originalToggle, 0);
                SystemParametersInfo(SPI_SETFILTERKEYS, (uint)Marshal.SizeOf<FILTERKEYS>(), ref _originalFilter, 0);
            }
            catch
            {
                // Best-effort restore; nothing more to do if even this fails.
            }
            finally
            {
                _disabled = false;
            }
        }
    }
}
