using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using log4net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Launcher.MessageHandlers;
using EnergonSoftware.Launcher.Net;

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

    sealed class ClientState
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClientState));

#region Singleton
        private static ClientState _instance = new ClientState();
        public static ClientState Instance { get { return _instance; } }
#endregion

        public OvermindSession _overmindSession;

        private string _username;
        public string Username { get { return _username; } set { _username = value; ClientState.Instance.NotifyPropertyChanged("Username"); } }

        private string _password;
        public string Password { get { return _password; } set { _password = value; ClientState.Instance.NotifyPropertyChanged("Password"); } }

        public string Ticket { get; set; }

#region Login Properties
        private bool _loggedIn = false;
        public bool LoggedIn
        {
            get { return _loggedIn; }
            set {
                _loggedIn = value;
                NotifyPropertyChanged("LoggedIn");
                NotifyPropertyChanged("NotLoggedIn");
            }
        }
#endregion

#region UI Helpers
        private bool _loggingIn = false;
        public bool LoggingIn
        {
            get { return _loggingIn; }
            set {
                _loggingIn = value;
                NotifyPropertyChanged("LoggingIn");
                NotifyPropertyChanged("CanLogin");
            }
        }

        public bool CanLogin { get { return !LoggingIn && !LoggedIn; } }
        public bool NotLoggedIn { get { return !LoggedIn; } }
#endregion

        public delegate void OnErrorHandler(string error);
        public event OnErrorHandler OnError;

        public void Error(string error)
        {
            _logger.Error("Encountered an error: " + error);

            if(null != OnError) {
                OnError(error);
            }
        }

        public void Error(Exception error)
        {
            Error(error.Message);
        }

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(/*[CallerMemberName]*/ string property/*=null*/)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion

        private ClientState()
        {
// TODO: These go to the sessions
            /*ConnectionManager.Instance.OnSocketError += OnSocketErrorCallback;

            ConnectionManager.Instance.OnConnectFailed += OnConnectFailedCallback;
            ConnectionManager.Instance.OnConnectSuccess += OnConnectSuccessCallback;

            ConnectionManager.Instance.OnDisconnect += OnDisconnectCallback;*/
        }
    }
}
