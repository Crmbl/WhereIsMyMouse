using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WhereIsMyMouse.Utils.Enums;
using WhereIsMyMouse.Utils.Structures;
using Point = WhereIsMyMouse.Utils.Structures.Point;

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

        [DllImport("user32.dll")]
        private static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, uint fWinIni);

        [DllImport("user32.dll")]
        static extern IntPtr LoadCursorFromFile(string lpFileName);

        #endregion //P/Invoke

        #region Constants

        //Mouse handle
        private const int WH_MOUSE_LL = 14;
        
        //Value to update cursor
        private const int SPI_SETCURSORS = 0x0057;

        //Normal cursor
        private static uint OCR_NORMAL = 32512;

        private static string CURSOR_NAME = "cursor.cur";

        #endregion //Constants

        #region Properties

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static LowLevelMouseProc _proc = HookCallback;

        private static IntPtr _hookId = IntPtr.Zero;

        private static Stopwatch _stopwatch;

        private static Timer _timer;

        private static Point _mousePosition;

        private static List<MouseMoves> _mouseMoves;

        public static MainWindow MainWindow { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Start the mouse hook.
        /// </summary>
        public static void Start()
        {
            _mouseMoves = new List<MouseMoves>();
            _stopwatch = new Stopwatch();
            _timer = new Timer {Interval = 1500};
            _timer.Tick += TimerOnTick;

            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookId = SetWindowsHookEx(WH_MOUSE_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        /// <summary>
        /// Handle timer event.
        /// </summary>
        private static void TimerOnTick(object sender, EventArgs e)
        {
            SystemParametersInfo(SPI_SETCURSORS, 0, null, 0);
            _timer.Stop();
        }

        /// <summary>
        /// Stop the mouse hook.
        /// </summary>
        public static void Stop()
        {
            UnhookWindowsHookEx(_hookId);
            SystemParametersInfo(SPI_SETCURSORS, 0, null, 0);
            _timer.Tick -= TimerOnTick;
        }

        /// <summary>
        /// Handle the callback of mouse event.
        /// </summary>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || (MouseMessages) wParam != MouseMessages.WM_MOUSEMOVE)
                return CallNextHookEx(_hookId, nCode, wParam, lParam);

            if (_mousePosition.X != default(int) && _mousePosition.Y != default(int))
            {
                var delta = ((MSLLHook)Marshal.PtrToStructure(lParam, typeof(MSLLHook))).Point.X - _mousePosition.X;
                if (delta > 0)
                {
                    if (!_mouseMoves.Any() || _mouseMoves.Last() != MouseMoves.RIGHT)
                        _mouseMoves.Add(MouseMoves.RIGHT);
                }
                else if (delta < 0)
                {
                    if (!_mouseMoves.Any() || _mouseMoves.Last() != MouseMoves.LEFT)
                        _mouseMoves.Add(MouseMoves.LEFT);
                }

                if (_mouseMoves.Count == 1)
                    _stopwatch.Restart();
            }

            _mousePosition = ((MSLLHook)Marshal.PtrToStructure(lParam, typeof(MSLLHook))).Point;

            if (_mouseMoves.Count == 10)
                GrowMouse();

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// Change the size of the mouse cursor.
        /// </summary>
        public static void GrowMouse()
        {
            if (_stopwatch.ElapsedMilliseconds <= 1200)
            {
                SetSystemCursor(LoadCursorFromFile(string.Concat(Environment.CurrentDirectory, "\\", CURSOR_NAME)), OCR_NORMAL);
                _timer.Start();
            }

            _mouseMoves.Clear();
        }

        #endregion //Methods
    }
}
