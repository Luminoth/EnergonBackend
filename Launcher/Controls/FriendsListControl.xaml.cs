using System.Windows;
using System.Windows.Controls;

using EnergonSoftware.Launcher.Friends;

namespace EnergonSoftware.Launcher.Controls
{
    /// <summary>
    /// Interaction logic for FriendsListControl.xaml
    /// </summary>
    public partial class FriendsListControl : UserControl
    {
        public FriendsListControl()
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
