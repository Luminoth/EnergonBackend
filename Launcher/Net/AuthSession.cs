using System.Net.Sockets;

using EnergonSoftware.Core;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;

using log4net;

namespace EnergonSoftware.Launcher.Net
{
    internal enum AuthenticationStage
    {
        NotAuthenticated,
        Begin,
        Challenge,
        Finalize,
        Authenticated,
    }

    internal sealed class AuthSession : Session
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthSession));

#region Events
        public delegate void OnAuthSuccessHandler();
        public event OnAuthSuccessHandler OnAuthSuccess;

        public delegate void OnAuthFailedHandler(string reason);
        public event OnAuthFailedHandler OnAuthFailed;
#endregion

        public AuthenticationStage AuthStage { get; private set; }
        public bool Authenticating { get { return AuthStage > AuthenticationStage.NotAuthenticated && AuthStage < AuthenticationStage.Authenticated; } }
        public bool Authenticated { get { return AuthenticationStage.Authenticated == AuthStage; } }

        public string RspAuth { get; private set; }

        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public AuthSession(SessionManager manager) : base(manager)
        {
            AuthStage = AuthenticationStage.NotAuthenticated;
        }

        private AuthSession(Socket socket, SessionManager manager) : base(socket, manager)
        {
            AuthStage = AuthenticationStage.NotAuthenticated;
        }

        protected override void OnRun(MessageProcessor processor)
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
            Logger.Info("Authenticating as user '" + ClientState.Instance.Username + "'...");

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
            Logger.Info("Authentication successful!");
            Logger.Debug("Ticket=" + ticket);

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
            Logger.Warn("Authentication failed: " + reason);

            AuthStage = AuthenticationStage.NotAuthenticated;
            ClientState.Instance.Password = null;

            if(null != OnAuthFailed) {
                OnAuthFailed(reason);
            }

            Disconnect();
        }
    }
}
