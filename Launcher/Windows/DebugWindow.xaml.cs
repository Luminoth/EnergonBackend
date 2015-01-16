using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using EnergonSoftware.WindowsUtil;

namespace EnergonSoftware.Launcher.Windows
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public static DebugWindow Instance { get { return App.Instance.DebugWindow; } }

        public static async Task AppendOutputTextAsync(string logEntry)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    DebugWindow.Instance.OutputText.AppendText(logEntry, LoggerColor.ParseColor(logEntry));
                    DebugWindow.Instance.OutputText.ScrollToEnd();
                }
            );
        }

        public DebugWindow()
        {
            InitializeComponent();
        }

#region Event Handlers
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(IsVisible) {
                OutputText.ScrollToEnd();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
#endregion
    }
}
