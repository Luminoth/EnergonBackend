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

#region UI Event Handlers
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
#endregion

#region Event Handlers
        private async void OnUpdateFinished(object sender, UpdateFinishedEventArgs e)
        {
            if(e.Success) {
                await MainWindow.ShowLoginPageAsync().ConfigureAwait(false);
            } else {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        CloseButton.IsEnabled = true;
                    }
                );
            }
        }
#endregion
    }
}
