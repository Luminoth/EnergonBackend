using System;
using System.Configuration;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher
{
    class AuthManager
    {
        public enum AuthenticationStage
        {
            NotAuthenticated,
            Begin,
            Challenge,
            Finalize,
            Authenticated,
        }

        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuthManager));

#region Singleton
        private static AuthManager _instance = new AuthManager();
        public static AuthManager Instance { get { return _instance; } }
#endregion

#region Events
        public delegate void OnAuthSuccessHandler();
        public event OnAuthSuccessHandler OnAuthSuccess;

        public delegate void OnAuthFailedHandler(string reason);
        public event OnAuthFailedHandler OnAuthFailed;
#endregion

        private AuthenticationStage _authStage = AuthenticationStage.NotAuthenticated;
        public AuthenticationStage AuthStage
        {
            get { return _authStage; }
            private set {
                _authStage = value;
                ClientState.Instance.NotifyPropertyChanged("Authenticating");
                ClientState.Instance.NotifyPropertyChanged("Authenticated");
            }
        }

        public bool Authenticating { get { return AuthStage > AuthenticationStage.NotAuthenticated && AuthStage < AuthenticationStage.Authenticated; } }
        public bool Authenticated { get { return AuthenticationStage.Authenticated == AuthStage; } }

        private string _username;
        public string Username { get { return _username; } set { _username = value; ClientState.Instance.NotifyPropertyChanged("Username"); } }

        private string _password;
        public string Password { get { return _password; } private set { _password = value; ClientState.Instance.NotifyPropertyChanged("Password"); } }

        public string RspAuth { get; private set; }

        public string Ticket { get; private set; }

        private AuthSession _session;

        // TODO: this is silly, don't pass the password in here
        // should use a more "proper" way of querying for it
        public AuthSession AuthConnect(string password)
        {
            Password = password;

            _session = new AuthSession();
            _session.OnConnectSuccess += OnConnectSuccessCallback;
            _session.OnConnectFailed += OnConnectFailedCallback;
            _session.ConnectAsync(ConfigurationManager.AppSettings["authHost"], Convert.ToInt32(ConfigurationManager.AppSettings["authPort"]));
            return _session;
        }

        private void BeginAuth()
        {
            _logger.Info("Authenticating as user '" + Username + "'...");

            AuthMessage message = new AuthMessage();
            message.MechanismType = AuthType.DigestSHA512;
            _session.SendMessage(message);

            AuthStage = AuthenticationStage.Begin;
        }

        internal void AuthResponse(string response, string rspAuth)
        {
            RspAuth = rspAuth;

            ResponseMessage message = new ResponseMessage();
            message.Response = response;
            _session.SendMessage(message);

            AuthStage = AuthenticationStage.Challenge;
        }

        internal void AuthFinalize()
        {
            ResponseMessage response = new ResponseMessage();
            _session.SendMessage(response);

            AuthStage = AuthenticationStage.Finalize;
        }

        internal void AuthSuccess(string ticket)
        {
            _logger.Info("Authentication successful!");
            _logger.Debug("Ticket=" + ticket);

            AuthStage = AuthenticationStage.Authenticated;
            Ticket = ticket;

            Password = null;
            RspAuth = null;

            if(null != OnAuthSuccess) {
                OnAuthSuccess();
            }

            _session.Disconnect();
            _session = null;
        }

        internal void AuthFailed(string reason)
        {
            _logger.Warn("Authentication failed: " + reason);

            AuthStage = AuthenticationStage.NotAuthenticated;
            Ticket = null;

            Password = null;
            RspAuth = null;

            if(null != OnAuthFailed) {
                OnAuthFailed(reason);
            }

            _session.Disconnect();
            _session = null;
        }

        private void OnConnectFailedCallback(SocketError error)
        {
            AuthFailed("Failed to connect to the authentication server: " + error);
        }

        private void OnConnectSuccessCallback()
        {
            BeginAuth();
        }

        private void OnSocketErrorCallback(string error)
        {
            _session.Disconnect();
        }

        private void OnDisconnectCallback(int socketId)
        {
            if(!Authenticated) {
                ClientState.Instance.Error("Auth server disconnected!");
            }
            _session = null;
        }

        private AuthManager()
        {
        }
    }
}
