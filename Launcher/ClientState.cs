using System.ComponentModel;
using System.Runtime.CompilerServices;

using log4net;

namespace EnergonSoftware.Launcher
{
    public enum AuthenticationStage
    {
        NotAuthenticated,
        Begin,
        Challenge,
        Finalize,
        Authenticated,
    }

    sealed class ClientState : INotifyPropertyChanged
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClientState));

#region Singleton
        private static ClientState _instance = new ClientState();
        public static ClientState Instance { get { return _instance; } }
#endregion

        private string _news = "news";
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
            set {
                _loggedIn = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("NotLoggedIn");
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

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string property=null)
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
