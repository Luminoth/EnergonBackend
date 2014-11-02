using System;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

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

            ClientState.Instance.OnAuthFailed += OnAuthFailedCallback;
            ClientState.Instance.OnAuthSuccess += OnAuthSuccessCallback;

            ClientState.Instance.OnLoginFailed += OnLoginFailedCallback;
            ClientState.Instance.OnLoginSuccess += OnLoginSuccessCallback;
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
        private void OnLogin(object sender, RoutedEventArgs evt)
        {
            ClientState.Instance.LoggingIn = true;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ClientState.Instance.AuthConnect(Password.Password);
            }));
        }

        private void OnAuthFailedCallback(string reason)
        {
            ClearPassword();
            ((App)Application.Current).OnError("Authentication failed: " + reason, "Authentication Failed");
            ClientState.Instance.LoggingIn = false;
        }

        private void OnAuthSuccessCallback()
        {
            ClearPassword();
            ClientState.Instance.OvermindConnect();
            ClientState.Instance.LoggingIn = false;
        }

        private void OnLoginFailedCallback(string reason)
        {
            ((App)Application.Current).OnError("Login failed: " + reason, "Login Failed");
            ClientState.Instance.LoggingIn = false;
        }

        private void OnLoginSuccessCallback()
        {
            ClientState.Instance.LoggingIn = false;
// TODO
        }
#endregion
    }
}
