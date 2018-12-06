using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using WhereIsMyMouse.Resources;
using WhereIsMyMouse.Utils.Enums;
using WhereIsMyMouse.Utils.Structures;
using Point = WhereIsMyMouse.Utils.Structures.Point;
// ReSharper disable InconsistentNaming

namespace WhereIsMyMouse.Utils
{
    /// <summary>
    /// Handle the mouse move feature.
    /// </summary>
    public static class MouseUtil
    {
        #region P/Invoke

        /// <summary>
        /// Hook the mouse events.
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// UnHook !
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// Needed to forward the hook... something like this.
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Get the handle of the program.
        /// </summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// Overrides the system cursor.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool SetSystemCursor(IntPtr hcur, uint id);

        /// <summary>
        /// Allows to reset the changed cursor to default.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, uint fWinIni);

        /// <summary>
        /// Allows to load an image then push into the SystemCursor.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        #endregion //P/Invoke

        #region Constants

        //Mouse handle.
        private const int WH_MOUSE_LL = 14;
        
        //Value to update cursor.
        private const int SPI_SETCURSORS = 0x0057;

        //Number of movements to engage the mouse grow.
        private static readonly int MOVE_COUNT = int.Parse(ConfigurationManager.AppSettings["MOVE_COUNT"]);

        //Treshold before the mouse won't grow, in milliseconds.
        private static readonly int TRESHOLD_MILLI = int.Parse(ConfigurationManager.AppSettings["TRESHOLD_MILLI"]);

        //Treshold before the mouseOverride is hidden, in milliseconds.
        private static readonly int TRESHOLD_HIDE_MOUSE = int.Parse(ConfigurationManager.AppSettings["TRESHOLD_HIDE_MOUSE"]);

        #endregion //Constants

        #region Properties

        /// <summary>
        /// Delegate to handle mouse events.
        /// </summary>
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Callback for the delegate.
        /// </summary>
        private static readonly LowLevelMouseProc Proc = HookCallback;

        /// <summary>
        /// Defines the hook id. Self explanatory.
        /// </summary>
        private static IntPtr _hookId = IntPtr.Zero;

        /// <summary>
        /// Defines the "timer" for the whole feature.
        /// </summary>
        private static Stopwatch _stopwatch;

        /// <summary>
        /// Defines a limit of time after which the mouse is reset.
        /// </summary>
        private static Timer _timer;

        /// <summary>
        /// The mouse position at T time.
        /// </summary>
        private static Point _mousePosition;

        /// <summary>
        /// The list of moves to check on.
        /// </summary>
        private static List<MouseMoves> _mouseMoves;

        /// <summary>
        /// The Window mouseOverride to place on top of the real cursor.
        /// </summary>
        private static MouseOverride _mouse;

        /// <summary>
        /// Path for the blank cursor.
        /// </summary>
        private static string _dummyMousePath;

        /// <summary>
        /// The "length" of all screen combined.
        /// </summary>
        private static double _maxLength;

        /// <summary>
        /// Defines if the <see cref="MouseUtil"/> class have been init().
        /// </summary>
        private static bool _initDone;

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Init all the variables.
        /// </summary>
        public static void Init()
        {
            _mouseMoves = new List<MouseMoves>();
            _stopwatch = new Stopwatch();
            _timer = new Timer { Interval = TRESHOLD_HIDE_MOUSE };
            _mouse = new MouseOverride();
            _maxLength = Screen.AllScreens.Select(x => x.Bounds.Width).Sum();
            _dummyMousePath = string.Concat(Environment.CurrentDirectory, @"\Resources\Images\blank.cur");
            _initDone = true;
        }

        /// <summary>
        /// Start the mouse hook.
        /// </summary>
        public static void Start()
        {
            if (!_initDone)
                throw new Exception($"Init method has not been called : Init = {_initDone}.");

            _timer.Tick += TimerOnTick;
            AnimationUtil.StoryboardScaleUp.Completed += StoryboardScaleUpOnCompleted;
            AnimationUtil.StoryboardScaleDown.Completed += StoryboardScaleDownOnCompleted;

            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                _hookId = SetWindowsHookEx(WH_MOUSE_LL, Proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static void StoryboardScaleUpOnCompleted(object sender, EventArgs eventArgs)
        {
            if (!(_mouse.FindName("MouseImage") is Image image)) return;
            if (!(image.FindName("ScaleTransform") is ScaleTransform scaleTransform)) return;

            scaleTransform.ScaleX = 1;
        }

        private static void StoryboardScaleDownOnCompleted(object sender, EventArgs eventArgs)
        {
            if (!(_mouse.FindName("MouseImage") is Image image)) return;
            if (!(image.FindName("ScaleTransform") is ScaleTransform scaleTransform)) return;

            scaleTransform.ScaleX = 0;
            _mouse.Hide();
            SystemParametersInfo(SPI_SETCURSORS, 0, null, 0);
            _timer.Stop();
            _mouseMoves.Clear();
        }

        /// <summary>
        /// Handle timer event.
        /// </summary>
        private static void TimerOnTick(object sender, EventArgs e)
        {
            AnimationUtil.ScaleDown(_mouse);
        }

        /// <summary>
        /// Stop the mouse hook.
        /// </summary>
        public static void Stop()
        {
            UnhookWindowsHookEx(_hookId);

            _mouse.Hide();
            SystemParametersInfo(SPI_SETCURSORS, 0, null, 0);
            _timer.Tick -= TimerOnTick;
        }

        /// <summary>
        /// Handle the callback of mouse event.
        /// </summary>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //If event is NOT mouseMove, forward the hook
            if (nCode < 0 || (MouseMessages) wParam != MouseMessages.WM_MOUSEMOVE)
                return CallNextHookEx(_hookId, nCode, wParam, lParam);

            //Handle the movement direction and the _mouseMoves list
            if (_mousePosition.X >= default(int) && _mousePosition.X < _maxLength && _mousePosition.Y != default(int))
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

            //Checks if the mouseOverride is visible and adapt its position.
            if (_mouse.IsVisible)
            {
                _mouse.Left = _mousePosition.X - _mouse.ActualWidth / 2;
                _mouse.Top = _mousePosition.Y - _mouse.ActualHeight / 2;
            }
            else
            {
                if (_mouseMoves.Count == MOVE_COUNT)
                    GrowMouse();
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// Apply mouseOverride on top of real cursor, which is set to blank.cur.
        /// </summary>
        private static void GrowMouse()
        {
            if (_stopwatch.ElapsedMilliseconds <= TRESHOLD_MILLI)
            {
                _mouse.Show();
                _mouse.Activate();
                _mouse.Left = _mousePosition.X - _mouse.ActualWidth / 2;
                _mouse.Top = _mousePosition.Y - _mouse.ActualHeight / 2;
                AnimationUtil.ScaleUp(_mouse);

                foreach (var cursor in (uint[]) Enum.GetValues(typeof(OCRCursors)))
                    SetSystemCursor(LoadImage(IntPtr.Zero, _dummyMousePath, 2, 1, 1, 0x00000010), cursor);

                _timer.Start();
            }

            _mouseMoves.Clear();
        }

        #endregion //Methods
    }
}
