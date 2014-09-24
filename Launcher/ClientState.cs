using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

using log4net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;

using EnergonSoftware.Launcher.MessageHandlers;

namespace EnergonSoftware.Launcher
{
    internal enum AuthenticationStage
    {
        NotAuthenticated,
        Begin,
        Challenge,
        Finalize,
        Authenticated,
    }

    sealed class ClientState : ClientApi, INotifyPropertyChanged
    {
#region Singleton
        private static ClientState _instance = new ClientState();
        public static ClientState Instance { get { return _instance; } }
#endregion

        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClientState));

        private object _lock = new object();

#region Authentication Events
        public delegate void OnAuthSuccessHandler();
        public event OnAuthSuccessHandler OnAuthSuccess;

        public delegate void OnAuthFailedHandler(string reason);
        public event OnAuthFailedHandler OnAuthFailed;
#endregion

        public delegate void OnLogoutHandler();
        public event OnLogoutHandler OnLogout;

#region Authentication Properties
        private volatile AuthenticationStage _authStage = AuthenticationStage.NotAuthenticated;

        private volatile string _username;
        private volatile string _password;

        internal AuthenticationStage AuthStage
        {
            get { return _authStage; }
            private set
            {
                lock(_lock) {
                    _authStage = value;
                    NotifyPropertyChanged("Authenticating");
                    NotifyPropertyChanged("Authenticated");
                }
            }
        }

        public bool Authenticating { get { return AuthStage > AuthenticationStage.NotAuthenticated && AuthStage < AuthenticationStage.Authenticated; } }
        public bool Authenticated { get { return AuthenticationStage.Authenticated == AuthStage; } }

        public string Username { get { return _username; } }
        internal string Password { get { return _password; } }
#endregion

#region UI Helpers
        public bool NotAuthenticated { get { return !Authenticated; } }
        public bool CanLogin { get { return !Connecting && !Connected && !Authenticating && !Authenticated; } }
#endregion

#region Authentication Methods
        public void BeginAuth(string username, string password)
        {
            lock(_lock) {
                Logout();

                _username = username;
                _password = password;
                _logger.Info("Authenticating as " + Username + "...");

                AuthMessage message = new AuthMessage();
                message.MechanismType = AuthType.DigestSHA512;
                SendMessage(message);

                AuthStage = AuthenticationStage.Begin;
            }
        }

        public void Logout()
        {
            /*lock(_lock) {
                _logging.Info("Logging out...");
                LogoutMessage message = new LogoutMessage();
                SendMessage(message);

                if(null != OnLogout) {
                    OnLogout();
                }

                AuthStage = AuthenticationStage.NotAuthenticated;
                _username = null;
                _password = null;
                _rspAuth = null;
                _sessionid = null;

                _account = new Account();
            }*/
        }
#endregion

        private bool HandleMessages()
        {
            lock(_lock) {
                // TODO: this needs to be a while loop
                try {
                    NetworkMessage message = NetworkMessage.Parse(Reader.Buffer, Formatter);
                    if(null != message) {
                        _logger.Debug("Parsed message type: " + message.Payload.Type);
                        MessageHandler.HandleMessage(message.Payload);
                    }
                } catch(Exception e) {
                    Error(e);
                    return false;
                }
                return true;
            }
        }

        public void Run()
        {
            if(!Poll()) {
                return;
            }

            if(!HandleMessages()) {
                return;
            }

            //Ping();
        }

#region Property Notifier
        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if("Authenticated".Equals(e.PropertyName, StringComparison.InvariantCultureIgnoreCase)) {
                NotifyPropertyChanged("NotAuthenticated");
            } else if("FriendCount".Equals(e.PropertyName, StringComparison.InvariantCultureIgnoreCase)) {
                NotifyPropertyChanged("FriendButtonText");
            }

            NotifyPropertyChanged("CanLogin");
            NotifyPropertyChanged(e.PropertyName);
        }

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
            ApiPropertyChanged += OnPropertyChanged;
        }
    }
}
