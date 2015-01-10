using System;
using System.Windows;

namespace EnergonSoftware.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public static DebugWindow Instance { get { return App.Instance.DebugWindow; } }

        public static void AppendOutputText(string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                DebugWindow.Instance.OutputText.AppendText(text);
            }));
        }

        public DebugWindow()
        {
            InitializeComponent();
        }

#region Event Handlers
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
#endregion
    }
}
