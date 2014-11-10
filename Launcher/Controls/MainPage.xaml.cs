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
            DataContext = ClientState.Instance;
            InitializeComponent();
        }

#region Event Handlers
        private void LogoutButton_Click(object sender, RoutedEventArgs evt)
        {
// TODO: just have this go through all of the session in the session manager
// and call "logout" on them (make this an overridable method in Session)
            //ClientState.Instance.Logout();
        }
#endregion
    }
}
