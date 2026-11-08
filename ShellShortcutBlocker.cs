using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace WebPageScreensaver
{
    /// <summary>
    /// Swallows the shell-level shortcuts that would let someone leave the screensaver without
    /// dismissing it: the Windows keys (Start menu, Win+D/E/R/X/Tab), Alt+Tab, Alt+Esc, Ctrl+Esc
    /// and Ctrl+Shift+Esc (Task Manager). explorer.exe handles these before any window message
    /// reaches this app, so <c>ProcessCmdKey</c> never sees them; a global low-level keyboard hook
    /// (<c>WH_KEYBOARD_LL</c>) is the only user-mode place to stop them.
    ///
    /// What still passes: plain Escape and Alt+F4 (the ways to exit), and Ctrl+Alt+Del, which no
    /// user-mode process can intercept.
    ///
    /// Why it is built the way it is (the risks recorded in unattended-display-hardening.md):
    /// - The hook runs on its OWN thread with its own message loop. Windows calls a low-level hook
    ///   on the thread that installed it, and silently removes a hook that takes too long
    ///   (LowLevelHooksTimeout). A WebView2 page can keep the UI thread busy; this thread does
    ///   nothing but answer the hook.
    /// - The callback does a table lookup and one GetAsyncKeyState call, and never throws: any
    ///   exception passes the key on unchanged.
    /// - The hook belongs to this process's thread, so Windows removes it when the process ends,
    ///   however it ends. A crash cannot leave the session's keyboard filtered.
    /// </summary>
    internal static class ShellShortcutBlocker
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int HC_ACTION = 0;
        private const uint LLKHF_ALTDOWN = 0x20;
        private const uint WM_QUIT = 0x0012;

        internal const int VK_TAB = 0x09;
        internal const int VK_ESCAPE = 0x1B;
        internal const int VK_LWIN = 0x5B;
        internal const int VK_RWIN = 0x5C;
        private const int VK_CONTROL = 0x11;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int ptX;
            public int ptY;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        // Held in a field: a delegate passed to native code must outlive the hook, or the GC
        // collects it and the next keystroke calls freed memory.
        private static LowLevelKeyboardProc? _proc;
        private static IntPtr _hook = IntPtr.Zero;
        private static Thread? _thread;
        private static uint _threadId;

        /// <summary>
        /// Whether a key event should be swallowed. Pure, so it can be tested without Windows.
        /// It answers the same for the key's down and up events, so the Windows key's release
        /// (which is what opens the Start menu) is swallowed along with its press.
        /// </summary>
        internal static bool ShouldSwallow(int vk, bool altDown, bool ctrlDown)
        {
            switch (vk)
            {
                case VK_LWIN:
                case VK_RWIN:
                    return true;              // Start menu and every Win+key shortcut
                case VK_TAB:
                    return altDown;           // Alt+Tab, Alt+Shift+Tab
                case VK_ESCAPE:
                    return altDown || ctrlDown; // Alt+Esc, Ctrl+Esc, Ctrl+Shift+Esc; plain Esc exits
                default:
                    return false;
            }
        }

        /// <summary>Best-effort; never throws, because a failure must not stop the screensaver
        /// from showing. Idempotent.</summary>
        public static void Start()
        {
            if (_thread != null)
            {
                return;
            }

            try
            {
                using var installed = new ManualResetEventSlim(false);
                _thread = new Thread(() => HookThread(installed))
                {
                    IsBackground = true,   // never keeps the process alive
                    Name = "ShellShortcutBlocker",
                };
                _thread.Start();
                // Wait for the install so the shortcuts are blocked before the first frame shows.
                installed.Wait(TimeSpan.FromSeconds(2));
            }
            catch
            {
                // The screensaver still runs; only these shortcuts stay open.
            }
        }

        /// <summary>Removes the hook and ends its thread. Never throws. Idempotent.</summary>
        public static void Stop()
        {
            Thread? thread = _thread;
            if (thread == null)
            {
                return;
            }

            try
            {
                if (_threadId != 0)
                {
                    PostThreadMessage(_threadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                }
                thread.Join(TimeSpan.FromSeconds(2));
            }
            catch
            {
            }
            finally
            {
                _thread = null;
            }
        }

        private static void HookThread(ManualResetEventSlim installed)
        {
            try
            {
                _threadId = GetCurrentThreadId();
                _proc = HookCallback;
                _hook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(null), 0);
                if (_hook == IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"WebPageScreensaver: keyboard hook not installed ({new Win32Exception(Marshal.GetLastWin32Error()).Message})");
                    return;
                }
            }
            finally
            {
                installed.Set();
            }

            try
            {
                // The hook is delivered through this thread's message loop; WM_QUIT from Stop()
                // makes GetMessage return 0, and -1 is an error. Either ends the loop.
                while (GetMessage(out _, IntPtr.Zero, 0, 0) > 0)
                {
                }
            }
            finally
            {
                UnhookWindowsHookEx(_hook);
                _hook = IntPtr.Zero;
                _threadId = 0;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode == HC_ACTION)
                {
                    var data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                    bool altDown = (data.flags & LLKHF_ALTDOWN) != 0;
                    bool ctrlDown = (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0;
                    if (ShouldSwallow((int)data.vkCode, altDown, ctrlDown))
                    {
                        return (IntPtr)1;
                    }
                }
            }
            catch
            {
                // Fall through: pass the key on unchanged.
            }

            return CallNextHookEx(_hook, nCode, wParam, lParam);
        }
    }
}
