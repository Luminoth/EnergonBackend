﻿using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Launcher.Net;
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

#region Event Handlers
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            Common.InitFilesystem();

            await UpdateManager.Instance.CheckForUpdatesAsync();

            // have to run this in a separate thread
            // so that we don't lock up the UI
            Logger.Info("Starting idle thread...");
            _idleTask = Task.Run(() => OnIdle(), _cancellationToken.Token);
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            Logger.Info("Exiting...");
            _cancellationToken.Cancel();

            // TODO: logout?

            await Sessions.DisconnectAllAsync();

            Logger.Info("Goodbye!");
        }

        private async void OnAuthFailedCallback(string reason)
        {
            await OnErrorAsync("Authentication failed: " + reason, "Authentication Failed").ConfigureAwait(false);
            ClientState.Instance.LoggingIn = false;
        }

        private async void OnAuthSuccessCallback()
        {
            if(ClientState.Instance.UseDummyNetwork) {
                await ClientState.Instance.OnLoggedInAsync(true).ConfigureAwait(false);
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

        private /*async*/ void OnDisconnectCallback(object sender, DisconnectEventArgs e)
        {
            Logger.Debug("Disconnected: " + e.Reason);
            //await OnErrorAsync(reason, "Disconnected!").ConfigureAwait(false);
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

            ClientState.Instance.Password = password;
            ClientState.Instance.LoggingIn = true;

            if(ClientState.Instance.UseDummyNetwork) {
                Logger.Debug("Faking network for testing...");
                OnAuthSuccessCallback();
                return;
            }

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

            await ClientState.Instance.OnLoggedInAsync(false).ConfigureAwait(false);
        }
    }
}
