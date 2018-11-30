﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
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

        #endregion //P/Invoke

        #region Constants

        private const int WH_MOUSE_LL = 14;

        #endregion //Constants

        #region Properties

        public static event EventHandler MouseAction = delegate {};

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static LowLevelMouseProc _proc = HookCallback;

        private static IntPtr _hookId = IntPtr.Zero;

        private static Timer _timer;

        private static Point _mousePosition;

        private static bool _isFirstMove;

        private static List<MouseMoves> _mouseMoves;

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Start the mouse hook.
        /// </summary>
        public static void Start()
        {
            _mouseMoves = new List<MouseMoves>();
            _isFirstMove = true;
            _timer = new Timer { Interval = 350 };
            _timer.Tick += TimerOnTick;

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
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || (MouseMessages) wParam != MouseMessages.WM_MOUSEMOVE || !_isFirstMove)
                return CallNextHookEx(_hookId, nCode, wParam, lParam);

            _isFirstMove = false;
            //Console.WriteLine("Hook");

            _timer.Stop();
            _timer.Start();

            var prevMousePosition = new Point();
            if (!_mousePosition.Equals(new Point()))
                prevMousePosition = _mousePosition;

            //var prevMousePosition = _mousePosition;
            _mousePosition = ((MSLLHook)Marshal.PtrToStructure(lParam, typeof(MSLLHook))).Point;

            Console.WriteLine("///////////////////////////////////////");
            Console.WriteLine("X " + _mousePosition.X);
            //Console.WriteLine("X' " + prevMousePosition.X);

            var test = _mousePosition.X - prevMousePosition.X;
            if (test >= 100)
                Console.WriteLine("LEFT");
            else if (test <= -100)
                Console.WriteLine("RIGHT");
            else
                Console.WriteLine("NONE");

            //MouseAction(null, new EventArgs());

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// Handle the idle mouse event.
        /// </summary>
        private static void TimerOnTick(object sender, EventArgs eventArgs)
        {
            _timer.Stop();
            _isFirstMove = true;
            _timer.Start();
        }

        #endregion //Methods

        public static void DoStuff()
        {
            //Console.WriteLine("TEST");
        }
    }
}