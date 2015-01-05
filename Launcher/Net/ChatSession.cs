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
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Launcher.MessageHandlers;

using log4net;

namespace EnergonSoftware.Launcher.Net
{
    internal sealed class ChatSession : Session
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatSession));

        private bool ShouldPing { get { return 0 == LastMessageTime ? false : Time.CurrentTimeMs > (LastMessageTime + Convert.ToInt64(ConfigurationManager.AppSettings["chatPingRate"])); } }

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new MessageHandlerFactory(); } }

        public ChatSession() : base()
        {
        }

        protected async override Task OnRunAsync()
        {
            await PingAsync().ConfigureAwait(false);
        }

        private void OnConnectFailedCallback(object sender, ConnectEventArgs e)
        {
            Error("Failed to connect to the overmind server: " + e.Error);
        }

        private async void OnConnectSuccessCallback(object sender, ConnectEventArgs e)
        {
            await LoginAsync().ConfigureAwait(false);

            await SetVisibilityAsync(Visibility.Online).ConfigureAwait(false);
        }

        public async Task BeginConnectAsync(string host, int port)
        {
            OnConnectSuccess += OnConnectSuccessCallback;
            OnConnectFailed += OnConnectFailedCallback;
            await ConnectAsync(host, port).ConfigureAwait(false);
        }

        private async Task LoginAsync()
        {
            await SendMessageAsync(new LoginMessage()
                {
                    Username = ClientState.Instance.Username,
                    SessionId = ClientState.Instance.Ticket,
                }
            ).ConfigureAwait(false);
        }

        public async Task LogoutAsync()
        {
            await SendMessageAsync(new LogoutMessage()
                {
                    Username = ClientState.Instance.Username,
                    SessionId = ClientState.Instance.Ticket,
                }
            ).ConfigureAwait(false);

            Disconnect();
        }

        public async Task PingAsync()
        {
            if(!ShouldPing) {
                return;
            }

            await SendMessageAsync(new PingMessage()).ConfigureAwait(false);
        }

        public async Task SetVisibilityAsync(Visibility visibility)
        {
            await SendMessageAsync(new VisibilityMessage()
                {
                    Username = ClientState.Instance.Username,
                    SessionId = ClientState.Instance.Ticket,
                    Visibility = visibility,
                }
            ).ConfigureAwait(false);
        }
    }
}
