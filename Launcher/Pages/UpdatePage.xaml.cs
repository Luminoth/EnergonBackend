using System.Windows;
using System.Windows.Controls;

using EnergonSoftware.Launcher.Updater;
using EnergonSoftware.Launcher.Windows;

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

            UpdateManager.Instance.OnUpdateFinished += OnUpdateFinished;
        }

#region Event Handlers
        private async void OnUpdateFinished(object sender, UpdateFinishedEventArgs e)
        {
            if(e.Success) {
                await MainWindow.ShowLoginPageAsync().ConfigureAwait(false);
            } else {
                CloseButton.IsEnabled = true;
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
#endregion
    }
}
