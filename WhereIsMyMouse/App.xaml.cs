using System.Threading;
using System.Windows;
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
        }

        private static void RegisterInstance()
        {
            DependencyInjectionUtil.RegisterInstance<IDispatcher>(new Dispatcher(Thread.CurrentThread));
        }
    }
}
