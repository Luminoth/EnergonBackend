using System.ComponentModel;
using System.Windows;

namespace EnergonSoftware.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public static SettingsWindow Instance { get { return App.Instance.SettingsWindow; } }

        public SettingsWindow()
        {
            InitializeComponent();
        }

#region UI Event Handlers
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
#endregion
    }
}
