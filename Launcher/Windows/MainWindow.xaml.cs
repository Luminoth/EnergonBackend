using System;
using System.Windows;

namespace EnergonSoftware.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = ClientState.Instance;
        }
    }
}
