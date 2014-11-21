using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using log4net;
using log4net.Config;

using EnergonSoftware.Core;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Launcher.MessageHandlers;
using EnergonSoftware.Launcher.Net;

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
        private static OvermindSession _overmindSession;

        private static void OnIdle()
        {
            _logger.Info("**Idle mark**");
            while(!_quit) {
                try {
                    _sessions.PollAndRun();
                    _sessions.Cleanup();
                } catch(Exception e) {
                    _logger.Info("Unhandled Exception!", e);
                    ((App)Application.Current).OnError(e.Message, "Unhandled Exception!");
                }

                Thread.Sleep(0);
            }
        }

#region Initialization
        private void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }

        private static void ConfigureThreading()
        {
            int workerThreads,ioThreads;
            ThreadPool.GetMinThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMinThreads(Int32.Parse(ConfigurationManager.AppSettings["minWorkerThreads"]), ioThreads);

            ThreadPool.GetMaxThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMaxThreads(Int32.Parse(ConfigurationManager.AppSettings["maxWorkerThreads"]), ioThreads);
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
            ConfigureThreading();
            Common.InitFilesystem();

            UpdateChecker.Instance.CheckForUpdates();

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

        private void OnAuthFailedCallback(string reason)
        {
            OnError("Authentication failed: " + reason, "Authentication Failed");
            ClientState.Instance.LoggingIn = false;
        }

        private void OnAuthSuccessCallback()
        {
            _overmindSession = new OvermindSession(_sessions);
            _overmindSession.OnDisconnect += OnDisconnectCallback;
            _overmindSession.OnError += OnErrorCallback;
            _overmindSession.BeginConnect(ConfigurationManager.AppSettings["overmindHost"], Convert.ToInt32(ConfigurationManager.AppSettings["overmindPort"]));
            _sessions.Add(_overmindSession);
        }

        private void OnDisconnectCallback(string reason)
        {
            _logger.Debug("Disconnected: " + reason);
            //OnError(reason, "Disconnected!");
        }

        private void OnErrorCallback(string error)
        {
            OnError(error, "Error!");
            Logout();
        }
#endregion

        public void Login(string password)
        {
            _logger.Info("Logging in...");

            ClientState.Instance.Password = password;
            ClientState.Instance.LoggingIn = true;

            AuthSession session = new AuthSession(_sessions);
            session.OnAuthFailed += OnAuthFailedCallback;
            session.OnAuthSuccess += OnAuthSuccessCallback;
            session.OnDisconnect += OnDisconnectCallback;
            session.OnError += OnErrorCallback;
            session.BeginConnect(ConfigurationManager.AppSettings["authHost"], Convert.ToInt32(ConfigurationManager.AppSettings["authPort"]));
            _sessions.Add(session);
        }

        public void Logout()
        {
            if(null != _overmindSession) {
                _logger.Info("Logging out...");
                _overmindSession.Logout();
                _overmindSession = null;
            }
        }
    }
}
