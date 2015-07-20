using System.Threading.Tasks;
using System.Windows;

using EnergonSoftware.Launcher.Pages;

namespace EnergonSoftware.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static MainWindow Instance => (MainWindow)Application.Current.MainWindow;

#region UI Helpers
        public static async Task ShowLoginPageAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Instance.MenuFileLogout.IsEnabled = false;
                    Instance.MainFrame.Navigate(new LoginPage());
                });
        }

        public static async Task ShowMainPageAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Instance.MenuFileLogout.IsEnabled = true;
                    Instance.MainFrame.Navigate(new MainPage());
                });
        }

        public static async Task NavigateBackAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Instance.MainFrame.GoBack();
                });
        }
#endregion

        public MainWindow()
        {
            InitializeComponent();
        }

#region UI Event Handlers
        private async void MenuFileLogout_Click(object sender, RoutedEventArgs e)
        {
            await App.Instance.LogoutAsync();
        }

        private void MenuFileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemToolsSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow.Instance.Show();
        }

        private void MenuHelpDebugWindow_Click(object sender, RoutedEventArgs e)
        {
            DebugWindow.Instance.Show();
        }

        private void MenuItemHelpAbout_Click(object sender, RoutedEventArgs e)
        {
        }
#endregion
    }
}
