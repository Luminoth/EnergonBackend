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

            //DataContext = ClientApiWrapper.Instance;
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

        private void BeginLogin()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                //ClientApi.Instance.BeginLogin(Username.Text, Password.Password);
            }));
        }
#endregion

#region Event Handlers
        private void OnLogin(object sender, RoutedEventArgs evt)
        {
            /*ClientApi.Instance.OnConnectFailed += OnConnectFailed;
            ClientApi.Instance.OnConnectSuccess += OnConnectSuccess;
            ClientApi.Instance.ConnectAsync(Configuration.Instance.Host, Configuration.Instance.Port);*/
        }

        private void OnConnectFailed(SocketError error)
        {
            ClearPassword();
            OnError("Failed to connect to the server: " + error, "Connection Failed");
        }

        private void OnConnectSuccess()
        {
            /*ClientApi.Instance.OnLoginFailed += OnLoginFailed;
            ClientApi.Instance.OnLoginSuccess += OnLoginSuccess;*/
            BeginLogin();
        }

        private void OnLoginFailed(string reason)
        {
            ClearPassword();
            OnError("Authentication failed: " + reason, "Authentication Failed");
        }

        private void OnLoginSuccess()
        {
            ClearPassword();
        }
#endregion
    }
}
