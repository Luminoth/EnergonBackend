using System.Windows;
using System.Windows.Controls;

namespace EnergonSoftware.Launcher.Controls
{
    /// <summary>
    /// Interaction logic for FriendsListPanel.xaml
    /// </summary>
    public partial class FriendsListPanel : UserControl
    {
        public FriendsListPanel()
        {
            InitializeComponent();
            DataContext = FriendListManager.Instance;
        }

#region UI Event Handlers
        private void ButtonFriends_Click(object sender, RoutedEventArgs e)
        {
            FriendList.Visibility = FriendList.IsVisible ? Visibility.Hidden : Visibility.Visible;
        }
#endregion
    }
}
