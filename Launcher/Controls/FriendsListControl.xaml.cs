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
/*
 * http://www.wpf-tutorial.com/treeview-control/simple-treeview/
 * http://www.dotnetperls.com/treeview-wpf
 * https://msdn.microsoft.com/en-us/library/system.windows.controls.treeview(v=vs.110).aspx
 * http://www.codeproject.com/Articles/124644/Basic-Understanding-of-Tree-View-in-WPF
 * http://www.codeproject.com/Articles/26288/Simplifying-the-WPF-TreeView-by-Using-the-ViewMode
 * http://stackoverflow.com/questions/3673173/wpf-treeview-databinding-hierarchal-data-with-mixed-types
 */

        public FriendsListControl()
        {
            InitializeComponent();
            DataContext = FriendListManager.Instance;

            FriendListView.Items.Add(FriendListManager.Instance.RootGroupEntry);
        }

#region UI Event Handlers
        private void ButtonFriends_Click(object sender, RoutedEventArgs e)
        {
            FriendList.Visibility = FriendList.IsVisible ? Visibility.Hidden : Visibility.Visible;
        }

        private void MenuItemAddFriend_Click(object sender, RoutedEventArgs e)
        {
        }
#endregion
    }
}
