using System;
using System.Configuration;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;
using EnergonSoftware.Backend.Net.Sessions;
using EnergonSoftware.Backend.Packet;

using EnergonSoftware.Core.Serialization.Formatters;

using EnergonSoftware.Launcher.MessageHandlers;

using log4net;

namespace EnergonSoftware.Launcher.Net
{
    internal sealed class OvermindSession : MessageNetworkSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OvermindSession));

        private DateTime LastPingTime { get; set; } = DateTime.MaxValue;

        private bool ShouldPing
        {
            get
            {
                // TODO: this check sucks, find a better way to do it
                if(DateTime.MaxValue.Equals(LastPingTime)) {
                    return false;
                }

                return DateTime.Now.Subtract(LastPingTime).Milliseconds > Convert.ToInt64(ConfigurationManager.AppSettings["overmindPingRate"]);
            }
        }

        public override string Name => "overmind";

        // TODO: make this configurable
        public override int MaxSessionReceiveBufferSize => 1024 * 1000 * 10;

        protected override string MessageFormatterType => BinaryNetworkFormatter.FormatterType;

        protected override string PacketType => NetworkPacket.PacketType;

        public override IMessageHandlerFactory MessageHandlerFactory => new MessageHandlerFactory();

        public OvermindSession() 
        {
            MessageReceivedEvent += App.Instance.MessageProcessor.MessageReceivedEventHandler;
        }

        /*protected async override Task OnRunAsync()
        {
            await PingAsync().ConfigureAwait(false);
        }*/

        public async Task ConnectAsync(string host, int port)
        {
            bool useIPv6 = Convert.ToBoolean(ConfigurationManager.AppSettings["useIPv6"]);

            try {
                Logger.Info("Connecting to overmind server...");
                await ConnectAsync(host, port, SocketType.Stream, ProtocolType.Tcp, useIPv6).ConfigureAwait(false);
                if(!IsConnected) {
                    await ErrorAsync("Failed to connect to the overmind server").ConfigureAwait(false);
                    return;
                }

                await LoginAsync().ConfigureAwait(false);
            } catch(SocketException e) {
                await ErrorAsync($"Failed to connect to the overmind server: {e.Message}").ConfigureAwait(false);
            }
        }

        private async Task LoginAsync()
        {
            Logger.Info("Logging in to overmind server...");

            await SendAsync(new LoginMessage()
                {
                    AccountName = App.Instance.UserAccount.AccountName,
                    SessionId = App.Instance.UserAccount.SessionId,
                }).ConfigureAwait(false);
        }

        public async Task LogoutAsync()
        {
            Logger.Info("Logging out of overmind server...");

            await SendAsync(new LogoutMessage()
                {
                    AccountName = App.Instance.UserAccount.AccountName,
                    SessionId = App.Instance.UserAccount.SessionId,
                }).ConfigureAwait(false);

            await DisconnectAsync().ConfigureAwait(false);
        }

        public async Task PingAsync()
        {
            if(!ShouldPing) {
                return;
            }

            await SendAsync(new PingMessage()).ConfigureAwait(false);
            LastPingTime = DateTime.Now;
        }
    }
}
