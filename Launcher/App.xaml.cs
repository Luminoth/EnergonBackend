using System.Windows;

using log4net;
using log4net.Config;

using EnergonSoftware.Core;

namespace EnergonSoftware.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(App));

        private void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }

#region Event Handlers
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            Common.InitFilesystem();
        }
#endregion
    }
}
