using System;
using System.Configuration;
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
        }

#region UI Helpers
        private void ClearPassword()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Password.Password = "";
            }));
        }

        private void OnError(string message, string title)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        private void BeginAuth()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ClientState.Instance.BeginAuth(Username.Text, Password.Password);
            }));
        }
#endregion

#region Event Handlers
        private void OnLogin(object sender, RoutedEventArgs evt)
        {
            ClientState.Instance.OnConnectFailed += OnConnectFailed;
            ClientState.Instance.OnConnectSuccess += OnConnectSuccess;
            ClientState.Instance.ConnectAsync(ConfigurationManager.AppSettings["authHost"], Int32.Parse(ConfigurationManager.AppSettings["authPort"]));
        }

        private void OnConnectFailed(SocketError error)
        {
            ClearPassword();
            OnError("Failed to connect to the server: " + error, "Connection Failed");
        }

        private void OnConnectSuccess()
        {
            ClientState.Instance.OnAuthFailed += OnAuthFailed;
            ClientState.Instance.OnAuthSuccess += OnAuthSuccess;
            BeginAuth();
        }

        private void OnAuthFailed(string reason)
        {
            ClearPassword();
            OnError("Authentication failed: " + reason, "Authentication Failed");
        }

        private void OnAuthSuccess()
        {
            ClearPassword();
        }
#endregion
    }
}
