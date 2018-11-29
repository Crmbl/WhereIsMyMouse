using System.ComponentModel;
using WhereIsMyMouse.Utils;

namespace WhereIsMyMouse
{
    /// <summary>
    /// MainWindow viewModel.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged

        /// <summary>
        /// The <see cref="INotifyPropertyChanged"/> event handler.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise the modification event.
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion //NotifyPropertyChanged

        #region Instance variables

        /// <summary>
        /// Private var for AppTitle.
        /// </summary>
        private string _appTitle;

        /// <summary>
        /// Private var for Version.
        /// </summary>
        private string _version;

        /// <summary>
        /// Private var for ToggleEnableLabel.
        /// </summary>
        private string _toggleEnableLabel;

        /// <summary>
        /// Private vor for IsEnabled.
        /// </summary>
        private bool _isEnabled;

        #endregion //Instance variables

        #region Properties

        /// <summary>
        /// Defines if the app feature is enabled or not.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;

                if (value)
                    MouseUtil.Start();
                else
                    MouseUtil.Stop();

                NotifyPropertyChanged("IsEnabled");
            }
        }

        /// <summary>
        /// Defines the version of the app.
        /// </summary>
        public string Version
        {
            get => _version;
            set
            {
                if (_version == value) return;
                _version = value;
                NotifyPropertyChanged("Version");
            }
        }

        /// <summary>
        /// Defines the app title.
        /// </summary>
        public string AppTitle
        {
            get => _appTitle;
            set
            {
                if (_appTitle == value) return;
                _appTitle = value;
                NotifyPropertyChanged("AppTitle");
            }
        }

        /// <summary>
        /// Defines the value for the toggleEnableLabel.
        /// </summary>
        public string ToggleEnableLabel
        {
            get => _toggleEnableLabel;
            set
            {
                if (_toggleEnableLabel == value) return;
                _toggleEnableLabel = value;
                NotifyPropertyChanged("ToggleEnableLabel");
            }
        }

        /// <summary>
        /// Exit the application on command.
        /// </summary>
        public ActionCommand ExitCommand { get; set; }

        /// <summary>
        /// Toggle between Enabled and Disabled.
        /// </summary>
        public ActionCommand ToggleEnableCommand { get; set; }

        #endregion //Properties

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainViewModel()
        {
            AppTitle = "Where's my Mouse!";
            #if DEBUG
            Version = "1.0d";
            #else
			Version = "1.0r";
            #endif

            IsEnabled = true;
            ToggleEnableLabel = "Disable";
            ExitCommand = new ActionCommand(KillApp);
            ToggleEnableCommand = new ActionCommand(ToggleEnableMethod);
            MouseUtil.MouseAction += MovingMouse;
        }

        #endregion //Constructors

        #region Methods

        /// <summary>
        /// Toggle between enabled and disabled.
        /// </summary>
        public void ToggleEnableMethod()
        {
            IsEnabled = !IsEnabled;
            ToggleEnableLabel = IsEnabled ? "Disable" : "Enable";
        }

        /// <summary>
        /// Kill the app.
        /// </summary>
        public void KillApp()
        {
            MouseUtil.Stop();
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// Handle the mouse moving event.
        /// </summary>
        private void MovingMouse(object sender, System.EventArgs e)
        {
            MouseUtil.DoStuff();
        }

        #endregion //Methods
    }
}
