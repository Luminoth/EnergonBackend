using System;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using EnergonSoftware.Launcher.Properties;
using EnergonSoftware.Launcher.Windows;

namespace EnergonSoftware.Launcher
{
    internal sealed class ClientState : INotifyPropertyChanged
    {
        public static readonly ClientState Instance = new ClientState();

        public bool UseDummyNetwork { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["dummyNetwork"]); } }

        public string FriendButtonText { get { return string.Format(Resources.FriendsLabel, FriendListManager.Instance.OnlineCount, FriendListManager.Instance.Total); } }

        // *** move these
        private string _news = "Checking for news updates...";
        public string News { get { return _news; } set { _news = value; NotifyPropertyChanged(); } }

        private string _username;
        public string Username { get { return _username; } set { _username = value; NotifyPropertyChanged(); } }

        private string _password;
        public string Password { get { return _password; } set { _password = value; NotifyPropertyChanged(); } }

        public string Ticket { get; set; }

        private bool _loggedIn = false;
        public bool LoggedIn
        {
            get { return _loggedIn; }
            private set {
                _loggedIn = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("NotLoggedIn");

                LoggingIn = false;
            }
        }
        public bool NotLoggedIn { get { return !LoggedIn; } }

        private bool _loggingIn = false;
        public bool LoggingIn
        {
            get { return _loggingIn; }
            set {
                _loggingIn = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("NotLoggingIn");
            }
        }
        public bool NotLoggingIn { get { return !LoggingIn; } }

        public async Task OnLoggedInAsync(bool loggedIn)
        {
            LoggedIn = loggedIn;

            if(LoggedIn) {
                await MainWindow.ShowMainPageAsync().ConfigureAwait(false);
            } else {
                await MainWindow.ShowLoginPageAsync().ConfigureAwait(false);
            }
        }

        public async Task OnUpdatedAsync()
        {
            await MainWindow.ShowLoginPageAsync().ConfigureAwait(false);
        }

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property=null)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion

        private ClientState()
        {
        }
    }
}
