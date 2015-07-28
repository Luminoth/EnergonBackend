using System;
using System.Configuration;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages.Auth;
using EnergonSoftware.Backend.Net.Sessions;
using EnergonSoftware.Backend.Packet;

using EnergonSoftware.Core.Serialization.Formatters;
using EnergonSoftware.Core.Util;

using EnergonSoftware.Launcher.MessageHandlers;

using log4net;

namespace EnergonSoftware.Launcher.Net
{
    internal sealed class AuthSession : MessageNetworkSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthSession));

#region Events
        public event EventHandler<AuthSuccessEventArgs> AuthSuccessEvent;
        public event EventHandler<AuthFailedEventArgs> AuthFailedEvent;
#endregion

        public string RspAuth { get; private set; }

        public override string Name => "auth";

        // TODO: make this configurable
        public override int MaxSessionReceiveBufferSize => 1024 * 1000 * 10;

        protected override string MessageFormatterType => BinaryNetworkFormatter.FormatterType;

        protected override string PacketType => NetworkPacket.PacketType;

        public override IMessageHandlerFactory MessageHandlerFactory => new MessageHandlerFactory();

        public AuthSession() 
        {
            MessageReceivedEvent += App.Instance.MessageProcessor.MessageReceivedEventHandler;
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
                await AuthFailedAsync($"Failed to connect to the authentication server: {e.Message}").ConfigureAwait(false);
            }
        }

        private async Task BeginAuthAsync()
        {
            Logger.Info($"Authenticating as user '{App.Instance.UserAccount.AccountName}'...");

            await SendAsync(new AuthMessage()
                {
                    MechanismType = AuthType.DigestSHA512,
                }).ConfigureAwait(false);

            App.Instance.AuthStage = AuthenticationStage.Begin;
        }

        public async Task AuthResponseAsync(string response, string rspAuth)
        {
            RspAuth = rspAuth;

            await SendAsync(new ResponseMessage()
                {
                    Response = response,
                }).ConfigureAwait(false);

            App.Instance.AuthStage = AuthenticationStage.Challenge;
        }

        public async Task AuthFinalizeAsync()
        {
            await SendAsync(new ResponseMessage()).ConfigureAwait(false);
            App.Instance.AuthStage = AuthenticationStage.Finalize;
        }

        public async Task AuthSuccessAsync(string ticket)
        {
            Logger.Info("Authentication successful!");
            Logger.Debug($"Ticket={ticket}");

            App.Instance.AuthStage = AuthenticationStage.Authenticated;
            App.Instance.UserAccount.SessionId = ticket;
            App.Instance.UserAccount.Password = null;

            AuthSuccessEvent?.Invoke(this, new AuthSuccessEventArgs());

            await DisconnectAsync().ConfigureAwait(false);
        }

        public async Task AuthFailedAsync(string reason)
        {
            Logger.Warn($"Authentication failed: {reason}");

            App.Instance.AuthStage = AuthenticationStage.NotAuthenticated;
            App.Instance.UserAccount.Password = null;

            AuthFailedEvent?.Invoke(this, new AuthFailedEventArgs() { Reason = reason });

            await DisconnectAsync().ConfigureAwait(false);
        }
    }
}
