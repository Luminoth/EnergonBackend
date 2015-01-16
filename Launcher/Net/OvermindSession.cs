using System;
using System.Configuration;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Chat;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Launcher.MessageHandlers;

using log4net;

namespace EnergonSoftware.Launcher.Net
{
    internal sealed class OvermindSession : Session
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OvermindSession));

        private bool ShouldPing { get { return 0 == LastMessageTime ? false : Time.CurrentTimeMs > (LastMessageTime + Convert.ToInt64(ConfigurationManager.AppSettings["overmindPingRate"])); } }

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override string FormatterType { get { return BinaryMessageFormatter.FormatterType; } }
        protected override IMessageHandlerFactory HandlerFactory { get { return new MessageHandlerFactory(); } }

        public OvermindSession() : base()
        {
        }

        protected async override Task OnRunAsync()
        {
            await PingAsync().ConfigureAwait(false);
        }

        public async Task ConnectAsync(string host, int port)
        {
            try {
                Logger.Info("Connecting to overmind server...");
                await ConnectAsync(host, port, SocketType.Stream, ProtocolType.Tcp).ConfigureAwait(false);
                if(!Connected) {
                    await ErrorAsync("Failed to connect to the overmind server").ConfigureAwait(false);
                    return;
                }

                await LoginAsync().ConfigureAwait(false);
            } catch(SocketException e) {
                ErrorAsync("Failed to connect to the overmind server: " + e.Message).Wait();
            }
        }

        private async Task LoginAsync()
        {
            Logger.Info("Logging in to overmind server...");

            await SendMessageAsync(new LoginMessage()
                {
                    Username = ClientState.Instance.Username,
                    SessionId = ClientState.Instance.Ticket,
                }
            ).ConfigureAwait(false);
        }

        public async Task LogoutAsync()
        {
            Logger.Info("Logging out of overmind server...");

            await SendMessageAsync(new LogoutMessage()
                {
                    Username = ClientState.Instance.Username,
                    SessionId = ClientState.Instance.Ticket,
                }
            ).ConfigureAwait(false);

            await DisconnectAsync().ConfigureAwait(false);
        }

        public async Task PingAsync()
        {
            if(!ShouldPing) {
                return;
            }

            await SendMessageAsync(new PingMessage()).ConfigureAwait(false);
        }
    }
}
