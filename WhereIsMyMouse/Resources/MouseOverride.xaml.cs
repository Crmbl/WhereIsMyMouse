using System;
using System.Configuration;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WhereIsMyMouse.Resources
{
    /// <summary>
    /// Interaction logic for MouseOverride.xaml
    /// </summary>
    public sealed partial class MouseOverride : Window
    {
        public MouseOverride()
        {
            InitializeComponent();

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(string.Concat(Environment.CurrentDirectory, @"\Resources\Images\cursor.png"));
            bitmap.EndInit();
            MouseImage.Source = bitmap;
            ScaleTransform.ScaleX = 0;

            ShowInTaskbar = false;
            Height = int.Parse(ConfigurationManager.AppSettings["MOUSE_SIZE"]);
            Width = int.Parse(ConfigurationManager.AppSettings["MOUSE_SIZE"]);
            Topmost = true;
        }
    }
}
