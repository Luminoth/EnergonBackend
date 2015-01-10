using System;
using System.Windows;

namespace EnergonSoftware.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get { return (MainWindow)Application.Current.MainWindow; } }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = ClientState.Instance;
        }

#region Event Handlers
        private void MenuHelpDebugWindow_Click(object sender, RoutedEventArgs evt)
        {
            DebugWindow.Instance.Show();
        }
#endregion
    }
}
