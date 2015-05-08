using System;
using System.Configuration;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages.Auth;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Packet;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.Launcher.MessageHandlers;

using log4net;

namespace EnergonSoftware.Launcher.Net
{
    internal sealed class AuthSession : NetworkSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthSession));

#region Events
        public event EventHandler<AuthSuccessEventArgs> AuthSuccessEvent;
        public event EventHandler<AuthFailedEventArgs> AuthFailedEvent;
#endregion

        public string RspAuth { get; private set; }

        public override string Name { get { return "auth"; } }

        private readonly NetworkPacketParser _messageParser = new NetworkPacketParser();
        private readonly MessageProcessor _messageProcessor = new MessageProcessor();
        private readonly IMessageHandlerFactory _messageHandlerFactory = new MessageHandlerFactory();

        protected override string FormatterType { get { return BinaryMessageFormatter.FormatterType; } }

        public AuthSession() : base()
        {
        }

        public async Task BeginConnectAsync(string host, int port)
        {
            bool useIPv6 = Convert.ToBoolean(ConfigurationManager.AppSettings["useIPv6"]);

            try {
                Logger.Info("Connecting to authentication server...");
                await ConnectAsync(host, port, SocketType.Stream, ProtocolType.Tcp, useIPv6).ConfigureAwait(false);
                if(!IsConnected) {
                    await ErrorAsync("Failed to connect to the authentication server").ConfigureAwait(false);
                    return;
                }

                await BeginAuthAsync().ConfigureAwait(false);
            } catch(SocketException e) {
                AuthFailedAsync("Failed to connect to the authentication server: " + e.Message).Wait();
            }
        }

        private async Task BeginAuthAsync()
        {
            Logger.Info("Authenticating as user '" + App.Instance.UserAccount.AccountName + "'...");

            await SendMessageAsync(new AuthMessage()
                {
                    MechanismType = AuthType.DigestSHA512,
                }).ConfigureAwait(false);

            App.Instance.AuthStage = AuthenticationStage.Begin;
        }

        public async Task AuthResponseAsync(string response, string rspAuth)
        {
            RspAuth = rspAuth;

            await SendMessageAsync(new ResponseMessage()
                {
                    Response = response,
                }).ConfigureAwait(false);

            App.Instance.AuthStage = AuthenticationStage.Challenge;
        }

        public async Task AuthFinalizeAsync()
        {
            await SendMessageAsync(new ResponseMessage()).ConfigureAwait(false);
            App.Instance.AuthStage = AuthenticationStage.Finalize;
        }

        public async Task AuthSuccessAsync(string ticket)
        {
            Logger.Info("Authentication successful!");
            Logger.Debug("Ticket=" + ticket);

            App.Instance.AuthStage = AuthenticationStage.Authenticated;
            App.Instance.UserAccount.SessionId = ticket;
            App.Instance.UserAccount.Password = null;

            if(null != AuthSuccessEvent) {
                AuthSuccessEvent(this, new AuthSuccessEventArgs());
            }

            await DisconnectAsync().ConfigureAwait(false);
        }

        public async Task AuthFailedAsync(string reason)
        {
            Logger.Warn("Authentication failed: " + reason);

            App.Instance.AuthStage = AuthenticationStage.NotAuthenticated;
            App.Instance.UserAccount.Password = null;

            if(null != AuthFailedEvent) {
                AuthFailedEvent(this, new AuthFailedEventArgs() { Reason = reason });
            }

            await DisconnectAsync().ConfigureAwait(false);
        }

        protected override MessagePacket CreatePacket(IMessage message)
        {
            return new NetworkPacket();
        }
    }
}
