using System;
using System.Windows;

namespace EnergonSoftware.DbInit.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get { return (MainWindow)Application.Current.MainWindow; } }

        public static void AppendOutputText(string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if(null != Application.Current.MainWindow) {
                    MainWindow.Instance.OutputText.AppendText(text);
                }
            }));
        }

        public static void SetStatusBarText(string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if(null != Application.Current.MainWindow) {
                    MainWindow.Instance.StatusBarText.Text = text;
                }
            }));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = Application.Current;

            SetStatusBarText("Waiting...");
        }

#region Event Handlers
        public void MenuFileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void MenuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, EnergonSoftware.DbInit.Properties.Resources.MainWindowTitle,
                "About " + EnergonSoftware.DbInit.Properties.Resources.MainWindowTitle,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async void ButtonInitialize_Click(object sender, RoutedEventArgs e)
        {
            OutputText.Clear();
            SetStatusBarText("Running...");

            App.Instance.Running = true;
            await DatabaseManager.InitializeDatabaseAsync().ConfigureAwait(false);
            App.Instance.Running = false;

            SetStatusBarText("Success!");
        }

        public void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            OutputText.Clear();
            SetStatusBarText("Waiting...");
        }
#endregion
    }
}
