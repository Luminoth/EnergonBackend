using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;
using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;
using EnergonSoftware.Backend.Messages.Chat;
using EnergonSoftware.Backend.Messages.Parser;
using EnergonSoftware.Backend.Net.Sessions;

using EnergonSoftware.Launcher.Friends;
using EnergonSoftware.Launcher.MessageHandlers;

using log4net;

namespace EnergonSoftware.Launcher.Net
{
    internal sealed class ChatSession : MessageSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatSession));

        private DateTime LastPingTime { get; set; } = DateTime.MaxValue;

        private bool ShouldPing
        {
            get
            {
                // TODO: this check sucks, find a better way to do it
                if(DateTime.MaxValue.Equals(LastPingTime)) {
                    return false;
                }

                return DateTime.Now.Subtract(LastPingTime).Milliseconds > Convert.ToInt64(ConfigurationManager.AppSettings["chatPingRate"]);
            }
        }

        public override string Name => "chat";

        private readonly NetworkPacketParser _messageParser = new NetworkPacketParser();
        private readonly MessageProcessor _messageProcessor = new MessageProcessor();
        private readonly IMessageHandlerFactory _messageHandlerFactory = new MessageHandlerFactory();

        protected override string FormatterType => BinaryMessageFormatter.FormatterType;

        /*protected async override Task OnRunAsync()
        {
            await PingAsync().ConfigureAwait(false);
        }*/

        public async Task ConnectAsync(string host, int port)
        {
            bool useIPv6 = Convert.ToBoolean(ConfigurationManager.AppSettings["useIPv6"]);

            try {
                Logger.Info("Connecting to chat server...");
                await ConnectAsync(host, port, SocketType.Stream, ProtocolType.Tcp, useIPv6).ConfigureAwait(false);
                if(!IsConnected) {
                    await ErrorAsync("Failed to connect to the chat server").ConfigureAwait(false);
                    return;
                }

                await LoginAsync().ConfigureAwait(false);
                await SetVisibilityAsync(Visibility.Online).ConfigureAwait(false);
            } catch(SocketException e) {
                await ErrorAsync($"Failed to connect to the chat server: {e.Message}").ConfigureAwait(false);
            }
        }

        private async Task LoginAsync()
        {
            Logger.Info("Logging in to chat server...");

            await SendMessageAsync(new LoginMessage()
                {
                    AccountName = App.Instance.UserAccount.AccountName,
                    SessionId = App.Instance.UserAccount.SessionId,
                }).ConfigureAwait(false);
        }

        public async Task LogoutAsync()
        {
            Logger.Info("Logging out of chat server...");

            await SendMessageAsync(new LogoutMessage()
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

            await SendMessageAsync(new PingMessage()).ConfigureAwait(false);
            LastPingTime = DateTime.Now;
        }

        public async Task SetVisibilityAsync(Visibility visibility)
        {
            Logger.Info($"Setting chat visibility={visibility}");

            await SendMessageAsync(new VisibilityMessage()
                {
                    AccountName = App.Instance.UserAccount.AccountName,
                    SessionId = App.Instance.UserAccount.SessionId,
                    Visibility = visibility,
                }).ConfigureAwait(false);
        }

        public void SetFriendList(IReadOnlyCollection<Account> friendList)
        {
            Logger.Info($"Received {friendList.Count} friends!");

            FriendListManager.Instance.Clear();
            FriendListManager.Instance.AddAll(friendList);

            Logger.Debug($"Friends: {FriendListManager.Instance}");
        }

        protected override MessagePacket CreatePacket(Message message)
        {
            return new NetworkPacket();
        }
    }
}
