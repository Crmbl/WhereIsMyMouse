using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using WhereIsMyMouse.Utils;
using Cursors = System.Windows.Input.Cursors;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace WhereIsMyMouse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties

        /// <summary>
        /// The system tray icon.
        /// </summary>
        private NotifyIcon NotifyIcon { get; set; }

        /// <summary>
        /// The viewmodel used for the app.
        /// </summary>
        private MainViewModel ViewModel { get; set; }

        #endregion //Properties

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            ViewModel = new MainViewModel();
            DataContext = ViewModel;

            #region System Tray Icon

            var resource = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Images/icon.ico"));
            if (resource == null) return;

            NotifyIcon = new NotifyIcon
            {
                Icon = new Icon(resource.Stream),
                Visible = false
            };

            NotifyIcon.DoubleClick += delegate
            {
                Show();
                WindowState = WindowState.Normal;
                NotifyIcon.Visible = false;
            };

            resource.Stream.Dispose();
            SetNotifyIconMenuItems();

            #endregion // System Tray Icon
        }

        #endregion //Constructors

        #region Events

        /// <summary>
        /// Event raised when the user click somewhere on the window and won't release click.
        /// </summary>
        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ((MainWindow)sender).Cursor = Cursors.None;
                DragMove();
            }
        }

        /// <summary>
        /// Event raised when the user release the mouse click.
        /// </summary>
        private void WindowMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)sender).Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Event raised when the mouse enter the window.
        /// </summary>
        private void WindowMouseEnter(object sender, MouseEventArgs e)
        {
            ((MainWindow)sender).Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Event raised when the mouse leave the window.
        /// </summary>
        private void WindowMouseLeave(object sender, MouseEventArgs e)
        {
            ((MainWindow)sender).Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Event raised on state changed.
        /// </summary>
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                NotifyIcon.Visible = true;
                Hide();
            }

            base.OnStateChanged(e);
        }

        /// <summary>
        /// Event raised when the use double click on the window.
        /// </summary>
        private void WindowMouseDouble(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Minimized;
                SetNotifyIconMenuItems();
            }
        }

        /// <summary>
        /// Event raised on source initialized.
        /// </summary>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            KeyUtil.Source = HwndSource.FromHwnd(helper.Handle);
            KeyUtil.Source?.AddHook(KeyUtil.HwndHook);
            KeyUtil.ViewModel = ViewModel;
            KeyUtil.RegisterKey(this);

            #if DEBUG
            ConsoleUtil.Show();
            #endif
        }

        /// <summary>
        /// Event raised on closed window.
        /// </summary>
        protected void OnClosed(object sender, EventArgs e)
        {
            KeyUtil.Source.RemoveHook(KeyUtil.HwndHook);
            KeyUtil.Source = null;
            KeyUtil.UnregisterKey(this);
            MouseUtil.Stop();
            
            #if DEBUG
            ConsoleUtil.Hide();
            #endif
        }

        #endregion //Events

        #region Methods

        /// <summary>
        /// Defines the NotifyIcon.ContextMenu.MenuItems.
        /// </summary>
        private void SetNotifyIconMenuItems()
        {
            if (NotifyIcon.ContextMenu == null)
                NotifyIcon.ContextMenu = new ContextMenu();

            NotifyIcon.ContextMenu.MenuItems.Clear();
            NotifyIcon.ContextMenu.MenuItems.Add("Maximize", (s, e) => { Show(); WindowState = WindowState.Normal; NotifyIcon.Visible = false; });
            NotifyIcon.ContextMenu.MenuItems.Add(ViewModel.ToggleEnableLabel, (s, e) => { ViewModel.ToggleEnableMethod(); SetNotifyIconMenuItems(); });
            NotifyIcon.ContextMenu.MenuItems.Add("Exit", (s, e) => ViewModel.KillApp());
        }

        #endregion //Methods
    }
}
