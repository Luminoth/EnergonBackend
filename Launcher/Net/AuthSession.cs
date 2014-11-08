using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Launcher.Net
{
    sealed class AuthSession : Session
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuthSession));

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

        public string RspAuth { get; private set; }

        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public AuthSession() : base()
        {
        }

        public AuthSession(Socket socket) : base(socket)
        {
        }

        protected override void OnRun()
        {
        }

        private void OnConnectFailedCallback(SocketError error)
        {
            AuthFailed("Failed to connect to the authentication server: " + error);
        }

        private void OnConnectSuccessCallback()
        {
            BeginAuth();
        }

        public void BeginConnect(string host, int port)
        {
            OnConnectSuccess += OnConnectSuccessCallback;
            OnConnectFailed += OnConnectFailedCallback;
            ConnectAsync(host, port);
        }

        private void BeginAuth()
        {
            _logger.Info("Authenticating as user '" + ClientState.Instance.Username + "'...");

            AuthMessage message = new AuthMessage();
            message.MechanismType = AuthType.DigestSHA512;
            SendMessage(message);

            AuthStage = AuthenticationStage.Begin;
        }

        public void AuthResponse(string response, string rspAuth)
        {
            RspAuth = rspAuth;

            ResponseMessage message = new ResponseMessage();
            message.Response = response;
            SendMessage(message);

            AuthStage = AuthenticationStage.Challenge;
        }

        public void AuthFinalize()
        {
            ResponseMessage response = new ResponseMessage();
            SendMessage(response);

            AuthStage = AuthenticationStage.Finalize;
        }

        public void AuthSuccess(string ticket)
        {
            _logger.Info("Authentication successful!");
            _logger.Debug("Ticket=" + ticket);

            AuthStage = AuthenticationStage.Authenticated;
            ClientState.Instance.Ticket = ticket;
            ClientState.Instance.Password = null;

            if(null != OnAuthSuccess) {
                OnAuthSuccess();
            }

            Disconnect();
        }

        public void AuthFailed(string reason)
        {
            _logger.Warn("Authentication failed: " + reason);

            AuthStage = AuthenticationStage.NotAuthenticated;
            ClientState.Instance.Password = null;

            if(null != OnAuthFailed) {
                OnAuthFailed(reason);
            }

            Disconnect();
        }
    }
}
