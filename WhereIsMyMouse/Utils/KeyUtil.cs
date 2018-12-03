using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;

namespace WhereIsMyMouse.Utils
{
    public static class KeyUtil
    {
        #region Constants

        private const int HOTKEY = 9010;

        #endregion //Constants

        #region P/Invoke

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey([In] IntPtr hWnd, [In] int id);

        #endregion //P/Invoke

        #region Properties

        /// <summary>
        /// Source of binding.
        /// </summary>
        public static HwndSource Source { get; set; }

        /// <summary>
        /// Viewmodel attached.
        /// </summary>
        public static MainViewModel ViewModel { get; set; }

        #endregion //Properties

        #region KeyManagement

        /// <summary>
        /// Register the toggle service key.
        /// </summary>
        public static void RegisterKey(MainWindow mainWindow)
        {
            var helper = new WindowInteropHelper(mainWindow);
            const uint VK_KEY = 0xDE;
            const uint MOD_CTRL = 0;
            if (!RegisterHotKey(helper.Handle, HOTKEY, MOD_CTRL, VK_KEY))
            {
                throw new Exception($"Error with binding to ² [{VK_KEY}]");
            }
        }

        /// <summary>
        /// Unregister the numpad hotkeys.
        /// </summary>
        public static void UnregisterKey(MainWindow mainWindow)
        {
            var helper = new WindowInteropHelper(mainWindow);
            UnregisterHotKey(helper.Handle, HOTKEY);
        }

        /// <summary>
		/// Hooks the key with a keypress event.
		/// </summary>
		public static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY:
                            ViewModel.IsEnabled = !ViewModel.IsEnabled;
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        #endregion // KeyManagement
    }
}
