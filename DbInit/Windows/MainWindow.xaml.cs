using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using EnergonSoftware.WindowsUtil;

namespace EnergonSoftware.DbInit.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get { return (MainWindow)Application.Current.MainWindow; } }

        public static async Task AppendOutputTextAsync(string logEntry)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if(null != Application.Current.MainWindow) {
                        MainWindow.Instance.OutputText.AppendText(logEntry, LoggerColor.ParseColor(logEntry));
                        MainWindow.Instance.OutputText.ScrollToEnd();
                    }
                });
        }

        public static async Task SetStatusBarTextAsync(string text)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if(null != Application.Current.MainWindow) {
                        MainWindow.Instance.StatusBarText.Text = text;
                    }
                });
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = Application.Current;
        }

#region Event Handlers
        private async void Window_Initialized(object sender, EventArgs e)
        {
            await SetStatusBarTextAsync("Waiting...");
        }

        public void MenuFileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void MenuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                this,
                EnergonSoftware.DbInit.Properties.Resources.MainWindowTitle,
                string.Format(EnergonSoftware.DbInit.Properties.Resources.AboutMessage, EnergonSoftware.DbInit.Properties.Resources.MainWindowTitle),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public async void ButtonInitialize_Click(object sender, RoutedEventArgs e)
        {
            ////OutputText.Document.Blocks.Clear();
            await SetStatusBarTextAsync("Running...");

            App.Instance.Running = true;
            await DatabaseManager.InitializeDatabaseAsync().ConfigureAwait(false);
            App.Instance.Running = false;

            await SetStatusBarTextAsync("Success!");
        }

        public async void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            OutputText.Document.Blocks.Clear();
            await SetStatusBarTextAsync("Waiting...");
        }
#endregion
    }
}
