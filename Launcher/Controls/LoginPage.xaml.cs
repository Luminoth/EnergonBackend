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
        private async void ButtonLogin_Click(object sender, RoutedEventArgs evt)
        {
            await ((App)Application.Current).LoginAsync(Password.Password);
            ClearPassword();
        }

        private async void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(IsVisible) {
                await NewsChecker.Instance.UpdateNewsAsync();
            }
        }
#endregion
    }
}
