using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Launcher.MessageHandlers;

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

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new MessageHandlerFactory(); } }

        public AuthSession() : base()
        {
            AuthStage = AuthenticationStage.NotAuthenticated;
        }

        private async void OnConnectFailedCallback(object sender, ConnectEventArgs e)
        {
            await AuthFailedAsync("Failed to connect to the authentication server: " + e.Error).ConfigureAwait(false);
        }

        private async void OnConnectSuccessCallback(object sender, ConnectEventArgs e)
        {
            await BeginAuthAsync().ConfigureAwait(false);
        }

        public async Task BeginConnectAsync(string host, int port)
        {
            OnConnectSuccess += OnConnectSuccessCallback;
            OnConnectFailed += OnConnectFailedCallback;
            await ConnectAsync(host, port).ConfigureAwait(false);
        }

        private async Task BeginAuthAsync()
        {
            Logger.Info("Authenticating as user '" + ClientState.Instance.Username + "'...");

            await SendMessageAsync(new AuthMessage()
                {
                    MechanismType = AuthType.DigestSHA512,
                }
            ).ConfigureAwait(false);

            AuthStage = AuthenticationStage.Begin;
        }

        public async Task AuthResponseAsync(string response, string rspAuth)
        {
            RspAuth = rspAuth;

            await SendMessageAsync(new ResponseMessage()
                {
                    Response = response,
                }
            ).ConfigureAwait(false);

            AuthStage = AuthenticationStage.Challenge;
        }

        public async Task AuthFinalizeAsync()
        {
            await SendMessageAsync(new ResponseMessage()).ConfigureAwait(false);
            AuthStage = AuthenticationStage.Finalize;
        }

        public async Task AuthSuccessAsync(string ticket)
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
            await Task.Delay(0).ConfigureAwait(false);
        }

        public async Task AuthFailedAsync(string reason)
        {
            Logger.Warn("Authentication failed: " + reason);

            AuthStage = AuthenticationStage.NotAuthenticated;
            ClientState.Instance.Password = null;

            if(null != OnAuthFailed) {
                OnAuthFailed(reason);
            }

            Disconnect();
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}
