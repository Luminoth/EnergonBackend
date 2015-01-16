using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EnergonSoftware.Launcher.Controls
{
    /// <summary>
    /// Interaction logic for FriendsListPage.xaml
    /// </summary>
    public partial class FriendsListPage : UserControl
    {
        public FriendsListPage()
        {
            InitializeComponent();
            DataContext = ClientState.Instance;
        }

#region Event Handlers
        private void ButtonFriends_Click(object sender, RoutedEventArgs e)
        {
            ClientState.Instance.ShowFriendsList = !ClientState.Instance.ShowFriendsList;
        }
#endregion
    }
}
