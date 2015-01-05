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

        private static readonly SessionManager Sessions = new SessionManager();
        private static OvermindSession _overmindSession;
        private static ChatSession _chatSession;

        private static volatile bool _quit;

        private static void OnIdle()
        {
            OnIdleAsync().Wait();
        }

        private static async Task OnIdleAsync()
        {
            Logger.Info("**Idle mark**");
            while(!_quit) {
                try {
                    await Sessions.PollAndRunAsync().ConfigureAwait(false);
                    Sessions.Cleanup();
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

            await UpdateChecker.Instance.CheckForUpdatesAsync();

            // have to run this in a separate thread
            // so that we don't lock up the UI
            Logger.Info("Starting idle thread...");
            await Task.Run(() => OnIdle());
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Logger.Info("Exiting...");
            _quit = true;

            // TODO: logout?

            Sessions.DisconnectAll();

            Logger.Info("Goodbye!");
        }

        private void OnAuthFailedCallback(string reason)
        {
            OnError("Authentication failed: " + reason, "Authentication Failed");
            ClientState.Instance.LoggingIn = false;
        }

        private async void OnAuthSuccessCallback()
        {
            _overmindSession = new OvermindSession();
            _overmindSession.OnDisconnect += OnDisconnectCallback;
            _overmindSession.OnError += OnErrorCallback;
            await _overmindSession.BeginConnectAsync(ConfigurationManager.AppSettings["overmindHost"], Convert.ToInt32(ConfigurationManager.AppSettings["overmindPort"])).ConfigureAwait(false);
            Sessions.Add(_overmindSession);

            _chatSession = new ChatSession();
            _chatSession.OnDisconnect += OnDisconnectCallback;
            _chatSession.OnError += OnErrorCallback;
            await _chatSession.BeginConnectAsync(ConfigurationManager.AppSettings["chatHost"], Convert.ToInt32(ConfigurationManager.AppSettings["chatPort"])).ConfigureAwait(false);
            Sessions.Add(_chatSession);
        }

        private void OnDisconnectCallback(object sender, DisconnectEventArgs e)
        {
            Logger.Debug("Disconnected: " + e.Reason);
            //OnError(reason, "Disconnected!");
        }

        private async void OnErrorCallback(object sender, ErrorEventArgs e)
        {
            OnError(e.Error, "Error!");
            await LogoutAsync().ConfigureAwait(false);
        }
#endregion

        public async Task LoginAsync(string password)
        {
            Logger.Info("Logging in...");

            ClientState.Instance.Password = password;
            ClientState.Instance.LoggingIn = true;

            AuthSession session = new AuthSession();
            session.OnAuthFailed += OnAuthFailedCallback;
            session.OnAuthSuccess += OnAuthSuccessCallback;
            session.OnDisconnect += OnDisconnectCallback;
            session.OnError += OnErrorCallback;
            await session.BeginConnectAsync(ConfigurationManager.AppSettings["authHost"], Convert.ToInt32(ConfigurationManager.AppSettings["authPort"])).ConfigureAwait(false);
            Sessions.Add(session);
        }

        public async Task LogoutAsync()
        {
            if(null != _overmindSession) {
                Logger.Info("Logging out...");

                await _chatSession.LogoutAsync().ConfigureAwait(false);
                _chatSession = null;

                await _overmindSession.LogoutAsync().ConfigureAwait(false);
                _overmindSession = null;
            }

            ClientState.Instance.LoggingIn = false;
            ClientState.Instance.LoggedIn = false;

            ClientState.Instance.CurrentPage = ClientState.Page.Login;
        }
    }
}
