using System.Windows.Controls;

using EnergonSoftware.Launcher.News;

namespace EnergonSoftware.Launcher.Controls
{
    /// <summary>
    /// Interaction logic for NewsControl.xaml
    /// </summary>
    public partial class NewsControl : UserControl
    {
        public NewsControl()
        {
            InitializeComponent();
            DataContext = NewsManager.Instance;
        }
    }
}
