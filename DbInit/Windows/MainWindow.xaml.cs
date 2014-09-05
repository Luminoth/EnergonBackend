using System;
using System.Diagnostics;
using System.Windows;

namespace EnergonSoftware.DbInit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private class OutputTraceListener : TraceListener
        {
            public OutputTraceListener()
            {
            }

            public override void Write(string message)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ((MainWindow)Application.Current.MainWindow).OutputText.AppendText(message);
                }));
            }

            public override void WriteLine(string message)
            {
                Write(message + Environment.NewLine);
            }
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

            Trace.Listeners.Add(new OutputTraceListener());
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
            App.InitializeDatabases();
        }
#endregion
    }
}
