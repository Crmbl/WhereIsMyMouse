using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
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

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        #endregion //P/Invoke

        #region Constants

        //Mouse handle
        private const int WH_MOUSE_LL = 14;
        
        //Value to update cursor
        private const int SPI_SETCURSORS = 0x0057;

        private const uint IMAGE_BITMAP = 0;

        private const uint IMAGE_ICON = 1;

        private const uint LR_LOADFROMFILE = 0x00000010;

        private const uint LR_DEFAULTCOLOR = 0x00000000;

        private static string CURSOR_NAME = "cursor.ico";//"cursor.png";

        #endregion //Constants

        #region Properties

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static LowLevelMouseProc _proc = HookCallback;

        private static IntPtr _hookId = IntPtr.Zero;

        private static Stopwatch _stopwatch;

        private static Timer _timer;

        private static Point _mousePosition;

        private static List<MouseMoves> _mouseMoves;

        private static Bitmap _updatedCursor;

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

            var cursorPath = string.Concat(Environment.CurrentDirectory, @"\Resources\Images\", CURSOR_NAME);
            var y = new Bitmap(cursorPath);

            //Bitmap intermediateBitmap = new Bitmap(32, 32);
            //Graphics intermediateGraphics = Graphics.FromImage(intermediateBitmap);
            //intermediateGraphics.DrawImage(y, intermediateBitmap.Width / 2 - y.Width / 2, intermediateBitmap.Height / 2 - y.Height / 2);

            var tmpBitmap = new Bitmap(256, 256);
            Graphics finalGraphics = Graphics.FromImage(tmpBitmap);
            //Graphics intermediateGraphics = Graphics.FromImage(y);
            //intermediateGraphics.DrawImage();
            //finalGraphics.ScaleTransform(1000, 1000);
            finalGraphics.DrawImage(y, 0,/*tmpBitmap.Width / 2 - y.Width / 2, tmpBitmap.Height / 2 - y.Height / 2*/ 0, tmpBitmap.Width, tmpBitmap.Height);
            _updatedCursor = tmpBitmap;

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
            _updatedCursor.Dispose();
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
        private static void GrowMouse()
        {
            if (_stopwatch.ElapsedMilliseconds <= 1200)
            {
                foreach (var cursor in (uint[]) Enum.GetValues(typeof(OCRCursors)))
                {
                    //SetSystemCursor(_updatedCursor.GetHicon(), cursor);
                    var cursorPath = string.Concat(Environment.CurrentDirectory, @"\Resources\Images\", CURSOR_NAME);
                    var tmp = LoadImage(IntPtr.Zero, cursorPath, IMAGE_ICON, 256, 256, LR_DEFAULTCOLOR | LR_LOADFROMFILE);
                    SetSystemCursor(tmp, cursor);
                }

                _timer.Start();
            }

            _mouseMoves.Clear();
        }

        #endregion //Methods
    }
}
