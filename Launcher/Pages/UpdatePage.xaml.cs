using System.Windows;
using System.Windows.Controls;

namespace EnergonSoftware.Launcher.Pages
{
    /// <summary>
    /// Interaction logic for UpdatePage.xaml
    /// </summary>
    public partial class UpdatePage : Page
    {
        public UpdatePage()
        {
            InitializeComponent();
            DataContext = UpdateManager.Instance;
        }

#region Event Handlers
        private void ButtonClose_Click(object sender, RoutedEventArgs evt)
        {
            Application.Current.MainWindow.Close();
        }
#endregion
    }
}
