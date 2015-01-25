using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

            DataContext = ClientState.Instance;

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

#region Event Handlers
        private async void ButtonLogin_Click(object sender, RoutedEventArgs evt)
        {
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
