using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using EnergonSoftware.Launcher.News;

namespace EnergonSoftware.Launcher.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();

            DataContext = App.Instance.UserAccount;

            if(!DesignerProperties.GetIsInDesignMode(this)) {
                IsVisibleChanged += IsVisible_Changed;
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

#region UI Event Handlers
        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            Username.IsEnabled = false;
            Password.IsEnabled = false;
            LoginProgress.Visibility = Visibility.Visible;
            LoginButton.IsEnabled = false;

            await App.Instance.LoginAsync(Password.Password);
            await ClearPasswordAsync();
        }

        private async void IsVisible_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(IsVisible) {
                await NewsManager.Instance.UpdateNewsAsync();
            }
        }
#endregion
    }
}
