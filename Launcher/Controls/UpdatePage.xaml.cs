using System.Windows;
using System.Windows.Controls;

namespace EnergonSoftware.Launcher.Controls
{
    /// <summary>
    /// Interaction logic for UpdatePage.xaml
    /// </summary>
    public partial class UpdatePage : UserControl
    {
        public UpdatePage()
        {
            InitializeComponent();
            DataContext = UpdateChecker.Instance;
        }

#region Event Handlers
        private void ButtonClose_Click(object sender, RoutedEventArgs evt)
        {
            Application.Current.MainWindow.Close();
        }
#endregion
    }
}
