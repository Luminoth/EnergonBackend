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
                ClientState.Instance.AuthConnect(Password.Password);
            }));
        }
#endregion

#region Event Handlers
        private void OnLogin(object sender, RoutedEventArgs evt)
        {
            BeginAuth();
        }

        private void OnAuthFailedCallback(string reason)
        {
            ClearPassword();
            OnError("Authentication failed: " + reason, "Authentication Failed");
        }

        private void OnAuthSuccessCallback()
        {
            ClearPassword();
        }
#endregion
    }
}
