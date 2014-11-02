using System;
using System.Configuration;
using System.Threading;
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

        private static volatile bool _quit = false;
        private static Thread _idleThread;

        public static void Quit()
        {
            _logger.Info("Quitting...");
            _quit = true;
            if(null != _idleThread) {
                _logger.Info("Waiting for idle thread to stop...");
                _idleThread.Join();
            }
            _logger.Info("Goodbye!");
        }

        private static void OnIdle()
        {
            _logger.Info("**Idle mark**");
            while(!_quit) {
                ClientState.Instance.Run();
                Thread.Sleep(0);
            }
        }

#region Initialization
        private void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }
#endregion

#region UI Helpers
        public void OnError(string message, string title)
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

            int workerThreads,ioThreads;
            ThreadPool.GetMinThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMinThreads(Int32.Parse(ConfigurationManager.AppSettings["minWorkerThreads"]), ioThreads);

            ThreadPool.GetMaxThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMaxThreads(Int32.Parse(ConfigurationManager.AppSettings["maxWorkerThreads"]), ioThreads);

            ClientState.Instance.OnError += OnErrorCallback;

            // have to run this in a separate thread
            // so that we don't lock up the UI
            _logger.Info("Starting idle thread...");
            _idleThread = new Thread(new ThreadStart(OnIdle));
            _idleThread.Start();
        }
#endregion

        private void OnErrorCallback(string error)
        {
            OnError(error, "Error!");
        }
    }
}
