using System.Windows;
using System.Windows.Controls;

namespace EnergonSoftware.Launcher.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            DataContext = ClientState.Instance;
        }

#region Event Handlers
        private async void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            await App.Instance.LogoutAsync();
        }
#endregion
    }
}
