using System.Windows;
using System.Windows.Controls;

namespace EnergonSoftware.Launcher.Controls
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            DataContext = ClientState.Instance;
        }

#region Event Handlers
        private void LogoutButton_Click(object sender, RoutedEventArgs evt)
        {
            ((App)Application.Current).Logout();
        }
#endregion
    }
}
