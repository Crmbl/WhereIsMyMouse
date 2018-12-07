using System.Threading;
using System.Windows;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using WhereIsMyMouse.Utils;
using WhereIsMyMouse.Utils.Interfaces;
using Dispatcher = WhereIsMyMouse.Utils.Dispatcher;

namespace WhereIsMyMouse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            RegisterInstance();
            AppCenter.Start("0128ff78-b179-4ac9-b3d7-930460282d13", typeof(Analytics));
        }

        /// <summary>
        /// "Init" the dispatcher.
        /// </summary>
        private static void RegisterInstance()
        {
            DependencyInjectionUtil.RegisterInstance<IDispatcher>(new Dispatcher(Thread.CurrentThread));
        }
    }
}
