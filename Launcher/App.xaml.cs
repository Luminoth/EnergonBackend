using System;
using System.Windows;
using System.Windows.Interop;

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

#region Initialization
        private void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }
#endregion

#region UI Helpers
        private void OnError(string message, string title)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }
#endregion

#region Event Handlers
        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            Common.InitFilesystem();

            /*ClientApi.Instance.Init(Configuration.Instance.MinWorkerThreads, Configuration.Instance.MaxWorkerThreads);
            ClientApi.Instance.OnDisconnect += OnDisconnect;*/

            ComponentDispatcher.ThreadIdle += OnIdle;
        }

        private void OnIdle(object sender, EventArgs evt)
        {
            //ClientApi.Instance.Run();
        }
#endregion
    }
}
