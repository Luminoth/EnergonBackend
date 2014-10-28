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

#region Notification Properties
        public const string NOTIFY_AUTHENTICATING = "Authenticating";
        public const string NOTIFY_AUTHENTICATED = "Authenticated";
        public const string NOTIFY_NOT_AUTHENTICATED = "NotAuthenticated";
        public const string NOTIFY_CAN_LOGIN = "CanLogin";
#endregion

#region Network Properties
        public int AuthSocketId { get; /*private*/ set; }
#endregion

#region Authentication Events
        public delegate void OnAuthSuccessHandler();
        public event OnAuthSuccessHandler OnAuthSuccess;

        public delegate void OnAuthFailedHandler(string reason);
        public event OnAuthFailedHandler OnAuthFailed;
#endregion

        private IMessageFormatter _formatter = new BinaryMessageFormatter();

#region Authentication Properties
        private volatile AuthenticationStage _authStage = AuthenticationStage.NotAuthenticated;

        internal AuthenticationStage AuthStage
        {
            get { return _authStage; }
            private set
            {
                lock(_lock) {
                    _authStage = value;

                    NotifyPropertyChanged(NOTIFY_AUTHENTICATING);
                    NotifyPropertyChanged(NOTIFY_AUTHENTICATED);
                    NotifyPropertyChanged(NOTIFY_NOT_AUTHENTICATED);
                    NotifyPropertyChanged(NOTIFY_CAN_LOGIN);
                }
            }
        }

        public bool Authenticating { get { return AuthStage > AuthenticationStage.NotAuthenticated && AuthStage < AuthenticationStage.Authenticated; } }
        public bool Authenticated { get { return AuthenticationStage.Authenticated == AuthStage; } }

        public string Username { get; private set; }
        internal string Password { get; private set; }

        internal string RspAuth { get; set; }
#endregion

#region UI Helpers
        public bool NotAuthenticated { get { return !Authenticated; } }
        public bool CanLogin { get { return !Authenticating && !Authenticated; } }
#endregion

#region Authentication Methods
        internal void BeginAuth(string username, string password)
        {
            if(AuthSocketId < 0) {
                return;
            }

            lock(_lock) {
                Username = username;
                Password = password;
                _logger.Info("Authenticating as user '" + Username + "'...");

                AuthMessage message = new AuthMessage();
                message.MechanismType = AuthType.DigestSHA512;
                SendMessage(AuthSocketId, message, _formatter);

                AuthStage = AuthenticationStage.Begin;
            }
        }

        internal void AuthResponse(string response)
        {
            if(AuthSocketId < 0) {
                return;
            }

            ResponseMessage message = new ResponseMessage();
            message.Response = response;
            SendMessage(AuthSocketId, message, _formatter);

            lock(_lock) {
                AuthStage = AuthenticationStage.Challenge;
            }
        }

        internal void AuthFinalize()
        {
            if(AuthSocketId < 0) {
                return;
            }

            ResponseMessage response = new ResponseMessage();
            SendMessage(AuthSocketId, response, _formatter);

            lock(_lock) {
                AuthStage = AuthenticationStage.Finalize;
            }
        }

        internal void AuthSuccess(string ticket)
        {
            lock(_lock) {
                _logger.Info("Authentication successful!");

                _logger.Debug("Ticket=" + ticket);
                Ticket = ticket;

                Password = null;
                RspAuth = null;

                AuthStage = AuthenticationStage.Authenticated;
            }

            if(null != OnAuthSuccess) {
                OnAuthSuccess();
            }

            Disconnect(AuthSocketId);
            AuthSocketId = -1;

            OnAuthSuccess = null;
            OnAuthFailed = null;
        }

        internal void AuthFailed(string reason)
        {
            lock(_lock) {
                _logger.Warn("Authentication failed: " + reason);

                Ticket = null;

                Password = null;
                RspAuth = null;

                AuthStage = AuthenticationStage.NotAuthenticated;
            }

            if(null != OnAuthFailed) {
                OnAuthFailed(reason);
            }

            Disconnect(AuthSocketId);
            AuthSocketId = -1;

            OnAuthSuccess = null;
            OnAuthFailed = null;
        }
#endregion

        private bool HandleAuthMessages()
        {
            lock(_lock) {
                // TODO: this needs to be a while loop
                try {
                    NetworkMessage message = NetworkMessage.Parse(GetSocketReader(AuthSocketId).Buffer, _formatter);
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

        private bool HandleMessages()
        {
            if(AuthSocketId > 0) {
                if(!HandleAuthMessages()) {
                    return false;
                }
            }

            return true;
        }

        private bool Poll()
        {
            if(AuthSocketId > 0) {
                if(!Poll(AuthSocketId)) {
                    return false;
                }
            }

            return true;
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
            // pass on the API notifications
            NotifyPropertyChanged(e.PropertyName);

            // have to push this notification
            // in case the connecting properties change
            // TODO: wrap this in an if statement
            NotifyPropertyChanged(NOTIFY_CAN_LOGIN);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(/*[CallerMemberName]*/ string property/*=null*/)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion

        private ClientState()
        {
            // watch for API notifications
            ApiPropertyChanged += OnPropertyChanged;
        }
    }
}
