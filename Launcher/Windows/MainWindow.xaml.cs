using System;
using System.Threading.Tasks;
using System.Windows;

using EnergonSoftware.Launcher.Pages;

namespace EnergonSoftware.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get { return (MainWindow)Application.Current.MainWindow; } }

#region UI Helpers
        public static async Task ShowLoginPageAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MainWindow.Instance.MenuFileLogout.IsEnabled = false;
                    MainWindow.Instance.MainFrame.Navigate(new LoginPage());
                }
            );
        }

        public static async Task ShowMainPageAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MainWindow.Instance.MenuFileLogout.IsEnabled = true;
                    MainWindow.Instance.MainFrame.Navigate(new MainPage());
                }
            );
        }

        public static async Task NavigateBackAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MainWindow.Instance.MainFrame.GoBack();
                }
            );
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

        private void MenuHelpDebugWindow_Click(object sender, RoutedEventArgs e)
        {
            DebugWindow.Instance.Show();
        }

        private void MenuItemHelpSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow.Instance.Show();
        }
#endregion
    }
}
