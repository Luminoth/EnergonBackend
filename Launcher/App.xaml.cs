using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using EnergonSoftware.Core;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Launcher.MessageHandlers;
using EnergonSoftware.Launcher.Net;

using log4net;
using log4net.Config;

namespace EnergonSoftware.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(App));

        private static volatile bool _quit;

        private static SessionManager _sessions = new SessionManager();
        private static OvermindSession _overmindSession;
        private static ChatSession _chatSession;

        private static void OnIdle()
        {
            Logger.Info("**Idle mark**");
            while(!_quit) {
                try {
                    _sessions.PollAndRun();
                    _sessions.Cleanup();
                } catch(Exception e) {
                    Logger.Info("Unhandled Exception!", e);
                    ((App)Application.Current).OnError(e.Message, "Unhandled Exception!");
                }

                Thread.Sleep(0);
            }
        }

        private void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }

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
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            Common.InitFilesystem();

            await UpdateChecker.Instance.CheckForUpdates();

            _sessions.Start(new MessageHandlerFactory());

            // have to run this in a separate thread
            // so that we don't lock up the UI
            Logger.Info("Starting idle thread...");
            await Task.Run(() => OnIdle());
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Logger.Info("Exiting...");
            _quit = true;

            _sessions.Stop();

            Logger.Info("Goodbye!");
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

            _chatSession = new ChatSession(_sessions);
            _chatSession.OnDisconnect += OnDisconnectCallback;
            _chatSession.OnError += OnErrorCallback;
            _chatSession.BeginConnect(ConfigurationManager.AppSettings["chatHost"], Convert.ToInt32(ConfigurationManager.AppSettings["chatPort"]));
            _sessions.Add(_chatSession);
        }

        private void OnDisconnectCallback(object sender, DisconnectEventArgs e)
        {
            Logger.Debug("Disconnected: " + e.Reason);
            //OnError(reason, "Disconnected!");
        }

        private void OnErrorCallback(object sender, ErrorEventArgs e)
        {
            OnError(e.Error, "Error!");
            Logout();
        }
#endregion

        public void Login(string password)
        {
            Logger.Info("Logging in...");

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
                Logger.Info("Logging out...");

                _chatSession.Disconnect();
                _chatSession = null;

                _overmindSession.Logout();
                _overmindSession.Disconnect();
                _overmindSession = null;
            }

            ClientState.Instance.LoggingIn = false;
            ClientState.Instance.LoggedIn = false;

            ClientState.Instance.CurrentPage = ClientState.Page.Login;
        }
    }
}
