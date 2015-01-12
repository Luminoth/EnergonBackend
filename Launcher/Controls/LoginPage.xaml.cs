using System;
using System.ComponentModel;
using System.Threading.Tasks;
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

            if(!DesignerProperties.GetIsInDesignMode(this)) {
                IsVisibleChanged += Visible_Changed;
            }
        }

#region UI Helpers
        private async Task ClearPasswordAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Password.Password = string.Empty;
                }
            );
        }
#endregion

#region Event Handlers
        private async void ButtonLogin_Click(object sender, RoutedEventArgs evt)
        {
            await App.Instance.LoginAsync(Password.Password);
            await ClearPasswordAsync();
        }

        private async void Visible_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(IsVisible) {
                await NewsChecker.Instance.UpdateNewsAsync();
            }
        }
#endregion
    }
}
