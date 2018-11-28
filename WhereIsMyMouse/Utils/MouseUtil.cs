using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WhereIsMyMouse.Utils.Enums;
using WhereIsMyMouse.Utils.Structures;

namespace WhereIsMyMouse.Utils
{
    /// <summary>
    /// Handle the mouse move feature.
    /// </summary>
    public static class MouseUtil
    {
        #region P/Invoke

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion //P/Invoke

        #region Constants

        private const int WH_MOUSE_LL = 14;

        #endregion //Constants

        #region Properties

        public static event EventHandler MouseAction = delegate {};

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static LowLevelMouseProc _proc = HookCallback;

        private static IntPtr _hookId = IntPtr.Zero;

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Start the mouse hook.
        /// </summary>
        public static void Start()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookId = SetWindowsHookEx(WH_MOUSE_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        /// <summary>
        /// Stop the mouse hook.
        /// </summary>
        public static void Stop()
        {
            UnhookWindowsHookEx(_hookId);
        }

        /// <summary>
        /// Handle the callback of mouse event.
        /// </summary>
        /// <returns></returns>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam)
            {
                //MSLLHook hookStruct = (MSLLHook)Marshal.PtrToStructure(lParam, typeof(MSLLHook));
                MouseAction(null, new EventArgs());
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        #endregion //Methods

        public static void DoStuff()
        {
            Console.WriteLine("TEST");
        }
    }
}
