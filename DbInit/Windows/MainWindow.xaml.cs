using System;
using System.Windows;

namespace EnergonSoftware.DbInit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static void AppendOutputText(string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ((MainWindow)Application.Current.MainWindow).OutputText.AppendText(text);
            }));
        }

        public static void SetStatusBarText(string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ((MainWindow)Application.Current.MainWindow).StatusBarText.Text = text;
            }));
        }

        public MainWindow()
        {
            InitializeComponent();

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

        public void ButtonInitialize_Click(object sender, RoutedEventArgs e)
        {
            ((App)(Application.Current)).InitializeDatabases();
        }
#endregion
    }
}
