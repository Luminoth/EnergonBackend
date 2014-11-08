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
            AuthManager.Instance.OnAuthFailed += OnAuthFailedCallback;
            AuthManager.Instance.OnAuthSuccess += OnAuthSuccessCallback;

            DataContext = ClientState.Instance;
            InitializeComponent();
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
            ((App)Application.Current).Sessions.AddSession(AuthManager.Instance.AuthConnect(Password.Password));
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
            ((App)Application.Current).Sessions.AddSession(ClientState.Instance.OvermindConnect());
        }
#endregion
    }
}
