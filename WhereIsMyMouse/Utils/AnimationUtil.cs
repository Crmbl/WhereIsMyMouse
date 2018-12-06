using System;
using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WhereIsMyMouse.Utils
{
    /// <summary>
    /// Animate the cursor !
    /// </summary>
    public static class AnimationUtil
    {
        #region Properties

        private static readonly DoubleAnimation ScaleUpAnim = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            FillBehavior = FillBehavior.Stop,
            BeginTime = TimeSpan.FromSeconds(0),
            Duration = new Duration(TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TRESHOLD_SCALING"], CultureInfo.InvariantCulture)))
        };

        private static readonly DoubleAnimation ScaleDownAnim = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            FillBehavior = FillBehavior.Stop,
            BeginTime = TimeSpan.FromSeconds(0),
            Duration = new Duration(TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TRESHOLD_SCALING"], CultureInfo.InvariantCulture)))
        };

        public static Storyboard StoryboardScaleUp { get; }

        public static Storyboard StoryboardScaleDown { get; }

        #endregion // Properties

        #region Constructors

        static AnimationUtil()
        {
            StoryboardScaleUp = new Storyboard();
            Storyboard.SetTargetName(StoryboardScaleUp, "ScaleTransform");
            Storyboard.SetTargetProperty(StoryboardScaleUp, new PropertyPath(ScaleTransform.ScaleXProperty));
            StoryboardScaleUp.Children.Add(ScaleUpAnim);

            StoryboardScaleDown = new Storyboard();
            Storyboard.SetTargetName(StoryboardScaleDown, "ScaleTransform");
            Storyboard.SetTargetProperty(StoryboardScaleDown, new PropertyPath(ScaleTransform.ScaleXProperty));
            StoryboardScaleDown.Children.Add(ScaleDownAnim);
        }

        #endregion // Constructors

        #region Methods

        /// <summary>
        /// Scale up a control.
        /// </summary>
        public static void ScaleUp(Window control)
        {
            if (!(control.FindName("MouseImage") is Image image)) return;
            StoryboardScaleUp.Begin(image, HandoffBehavior.Compose);
        }

        /// <summary>
        /// Scale down a control.
        /// </summary>
        public static void ScaleDown(Window control)
        {
            if (!(control.FindName("MouseImage") is Image image)) return;
            StoryboardScaleDown.Begin(image, HandoffBehavior.Compose);
        }

        #endregion // Methods
    }
}
