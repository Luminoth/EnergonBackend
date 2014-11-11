using System;
using System.Configuration;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.Controls
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : UserControl
    {
        public LoginPage()
        {
            InitializeComponent();
            DataContext = ClientState.Instance;
        }

#region UI Helpers
        private void ClearPassword()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Password.Password = "";
            }));
        }
#endregion

#region Event Handlers
        private void ButtonLogin_Click(object sender, RoutedEventArgs evt)
        {
            ClientState.Instance.LoggingIn = true;
            ClientState.Instance.Password = Password.Password;
            ClearPassword();
  
            AuthSession session = new AuthSession();
            session.OnAuthFailed += OnAuthFailedCallback;
            session.OnAuthSuccess += OnAuthSuccessCallback;
// TODO: add disconnect/error callbacks
            session.BeginConnect(ConfigurationManager.AppSettings["authHost"], Convert.ToInt32(ConfigurationManager.AppSettings["authPort"]));
            ((App)Application.Current).Sessions.AddSession(session);
        }

        private void OnAuthFailedCallback(string reason)
        {
            ((App)Application.Current).OnError("Authentication failed: " + reason, "Authentication Failed");
            ClientState.Instance.LoggingIn = false;
        }

        private void OnAuthSuccessCallback()
        {
            OvermindSession session = new OvermindSession();
// TODO: add disconnect/error callbacks
            session.BeginConnect(ConfigurationManager.AppSettings["overmindHost"], Convert.ToInt32(ConfigurationManager.AppSettings["overmindPort"]));
            ((App)Application.Current).Sessions.AddSession(session);
        }
#endregion
    }
}
