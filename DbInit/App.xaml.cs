using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

using EnergonSoftware.Core;

using log4net;
using log4net.Config;

namespace EnergonSoftware.DbInit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        public static App Instance { get { return (App)Application.Current; } }

        private static void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }

        private bool _running = false;
        public bool Running { get { return _running; } set { _running = value; NotifyPropertyChanged(); NotifyPropertyChanged("NotRunning"); } }
        public bool NotRunning { get { return !Running; } }

#region Event Handlers
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            await Common.InitFilesystemAsync();

            /*if(!DatabaseManager.TestDatabaseConnectionAsync().Result) {
                Logger.Fatal("Could not connect to database!");
                MessageBox.Show("Could not connect to database!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }*/
        }
#endregion

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string property=null)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion
    }
}
