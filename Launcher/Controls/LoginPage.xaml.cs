using System;
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

            IsVisibleChanged += OnVisibleChanged;
        }

#region UI Helpers
        private void ClearPassword()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Password.Password = string.Empty;
            }));
        }
#endregion

#region Event Handlers
        private void ButtonLogin_Click(object sender, RoutedEventArgs evt)
        {
            ((App)Application.Current).Login(Password.Password);
            ClearPassword();
        }

        private async void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(IsVisible) {
                await NewsChecker.Instance.UpdateNews();
            }
        }
#endregion
    }
}
