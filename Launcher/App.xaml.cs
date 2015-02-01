using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Launcher.Net;
using EnergonSoftware.Launcher.Updater;
using EnergonSoftware.Launcher.Windows;

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

        public static App Instance { get { return (App)Application.Current; } }

#region Window Properties
        public readonly DebugWindow DebugWindow = new DebugWindow();
        public readonly SettingsWindow SettingsWindow = new SettingsWindow();
#endregion

#region Network Properties
        private static readonly SessionManager Sessions = new SessionManager();
        private static OvermindSession _overmindSession;
        private static ChatSession _chatSession;
#endregion

        public Account UserAccount { get; private set; }

#region Auth Properties
        // TODO: move these into a model object
        public AuthenticationStage AuthStage { get; set; }
        public bool Authenticating { get { return AuthStage > AuthenticationStage.NotAuthenticated && AuthStage < AuthenticationStage.Authenticated; } }
        public bool Authenticated { get { return AuthenticationStage.Authenticated == AuthStage; } }
#endregion

#region Debug Properties
        public bool UseDummyNetwork
        {
            get
            {
#if DEBUG
                return Convert.ToBoolean(ConfigurationManager.AppSettings["dummyNetwork"]);
#else
                return false;
#endif
            }
        }
#endregion

#region Idle Properties
        private Task _idleTask;
        private static CancellationTokenSource _cancellationToken = new CancellationTokenSource();
#endregion

#region Idle Handlers
        private static void OnIdle()
        {
            OnIdleAsync().Wait();
        }

        private static async Task OnIdleAsync()
        {
            Logger.Debug("**Idle mark**");
            while(!_cancellationToken.IsCancellationRequested) {
                try {
                    await Sessions.PollAndRunAsync().ConfigureAwait(false);
                    Sessions.Cleanup();
                } catch(Exception e) {
                    Logger.Fatal("Unhandled Exception!", e);
                    App.Instance.OnErrorAsync(e.Message, "Unhandled Exception!").Wait();
                }

                await Task.Delay(0).ConfigureAwait(false);
            }
        }
#endregion

        private static void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }

        public App()
        {
            InitializeComponent();

            UserAccount = new Account();
            AuthStage = AuthenticationStage.NotAuthenticated;
        }

#region UI Helpers
        public async Task OnErrorAsync(string message, string title)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            );
            await LogoutAsync().ConfigureAwait(false);
        }
#endregion

#region UI Event Handlers
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            await Common.InitFilesystemAsync();

            // have to run this in a separate thread
            // so that we don't lock up the UI
            Logger.Info("Starting idle thread...");
            _idleTask = Task.Run(() => OnIdle(), _cancellationToken.Token);

            await UpdateManager.Instance.CheckForUpdatesAsync();
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            Logger.Info("Exiting...");
            _cancellationToken.Cancel();

            // TODO: logout?

            await Sessions.DisconnectAllAsync();

            Logger.Info("Goodbye!");
        }
#endregion

#region Event Handlers
        private async void OnAuthFailedCallback(string reason)
        {
            await OnErrorAsync("Authentication failed: " + reason, "Authentication Failed").ConfigureAwait(false);
        }

        private async void OnAuthSuccessCallback()
        {
            if(UseDummyNetwork) {
                await EnergonSoftware.Launcher.Windows.MainWindow.ShowMainPageAsync().ConfigureAwait(false);
                return;
            }

            _overmindSession = new OvermindSession();
            _overmindSession.OnDisconnect += OnDisconnectCallback;
            _overmindSession.OnError += OnErrorCallback;
            await _overmindSession.ConnectAsync(ConfigurationManager.AppSettings["overmindHost"], Convert.ToInt32(ConfigurationManager.AppSettings["overmindPort"])).ConfigureAwait(false);
            Sessions.Add(_overmindSession);

            _chatSession = new ChatSession();
            _chatSession.OnDisconnect += OnDisconnectCallback;
            _chatSession.OnError += OnErrorCallback;
            await _chatSession.ConnectAsync(ConfigurationManager.AppSettings["chatHost"], Convert.ToInt32(ConfigurationManager.AppSettings["chatPort"])).ConfigureAwait(false);
            Sessions.Add(_chatSession);
        }

        private async void OnDisconnectCallback(object sender, DisconnectEventArgs e)
        {
            AuthSession authSession = sender as AuthSession;
            if(null != authSession && Authenticated) {
                Logger.Debug("Ignoring expected auth session disconnect");
                return;
            }

            Session session = (Session)sender;
            Logger.Debug("Session " + session.Name + " disconnected: " + e.Reason);
            await OnErrorAsync(e.Reason, "Disconnected!").ConfigureAwait(false);
        }

        private async void OnErrorCallback(object sender, ErrorEventArgs e)
        {
            await OnErrorAsync(e.Error, "Error!").ConfigureAwait(false);
            await LogoutAsync().ConfigureAwait(false);
        }
#endregion

        public async Task LoginAsync(string password)
        {
            Logger.Info("Logging in...");

            AuthSession session = new AuthSession();
            session.OnAuthFailed += OnAuthFailedCallback;
            session.OnAuthSuccess += OnAuthSuccessCallback;
            session.OnDisconnect += OnDisconnectCallback;
            session.OnError += OnErrorCallback;

            AuthStage = AuthenticationStage.NotAuthenticated;
            UserAccount.Password = password;

            if(UseDummyNetwork) {
                Logger.Debug("Faking network for testing...");
                await Task.Delay(1000).ConfigureAwait(false);
                await session.AuthSuccessAsync(string.Empty).ConfigureAwait(false);

                return;
            }

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

            await EnergonSoftware.Launcher.Windows.MainWindow.ShowLoginPageAsync().ConfigureAwait(false);
        }
    }
}
