using System;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

using log4net;
using log4net.Config;

using EnergonSoftware.Core;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Launcher.MessageHandlers;

namespace EnergonSoftware.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(App));

        private static volatile bool _quit;
        private static Thread _idleThread;

        private static SessionManager _sessions = new SessionManager();
        public SessionManager Sessions { get { return _sessions; } }

        private static void OnIdle()
        {
            _logger.Info("**Idle mark**");
            while(!_quit) {
                try {
                    _sessions.PollAndRun();
                    _sessions.Cleanup();
                } catch(Exception e) {
                    _logger.Info("Unhandled Exception!", e);
                    ClientState.Instance.Error(e);
                }

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
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            Common.InitFilesystem();

            int workerThreads,ioThreads;
            ThreadPool.GetMinThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMinThreads(Int32.Parse(ConfigurationManager.AppSettings["minWorkerThreads"]), ioThreads);

            ThreadPool.GetMaxThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMaxThreads(Int32.Parse(ConfigurationManager.AppSettings["maxWorkerThreads"]), ioThreads);

            ClientState.Instance.OnError += OnErrorCallback;

            _sessions.Start(new MessageHandlerFactory());

            // have to run this in a separate thread
            // so that we don't lock up the UI
            _logger.Info("Starting idle thread...");
            _idleThread = new Thread(new ThreadStart(OnIdle));
            _idleThread.Start();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _logger.Info("Exiting...");

            _sessions.Stop();

            _quit = true;
            if(null != _idleThread) {
                _logger.Info("Waiting for idle thread to stop...");
                _idleThread.Join();
            }

            _logger.Info("Goodbye!");
        }
#endregion

        private void OnErrorCallback(string error)
        {
            OnError(error, "Error!");
        }
    }
}
