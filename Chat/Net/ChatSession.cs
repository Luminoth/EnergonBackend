using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;
using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Chat;
using EnergonSoftware.Backend.Messages.Parser;
using EnergonSoftware.Backend.Net.Sessions;

using EnergonSoftware.Chat.MessageHandlers;

using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.DAL;
using EnergonSoftware.DAL.Models.Accounts;

using log4net;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class ChatSessionFactory : INetworkSessionFactory
    {
        public NetworkSession Create(Socket socket)
        {
            ChatSession session = null;
            try {
                session = new ChatSession(socket)
                {
                    TimeoutMs = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]),
                };
                return session;
            } catch(Exception) {
                session?.Dispose();
                throw;
            }
        }
    }

    internal sealed class ChatSession : AuthenticatedSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatSession));

        public override string Name => "chat";

        private readonly NetworkPacketParser _messageParser = new NetworkPacketParser();
        private readonly MessageProcessor _messageProcessor = new MessageProcessor();
        private readonly IMessageHandlerFactory _messageHandlerFactory = new ChatMessageHandlerFactory();

        protected override string FormatterType => BinaryMessageFormatter.FormatterType;

        public ChatSession(Socket socket) : base(socket)
        {
        }

        protected async override Task<Account> LookupAccountAsync(string accountName)
        {
            Logger.Debug($"Looking up account for accountName={accountName}");
            using(AccountsDatabaseContext context = new AccountsDatabaseContext()) {
                var accounts = from a in context.Accounts where a.AccountName == accountName select a;
                if(accounts.Any()) {
                    return null;
                }

                await Task.Delay(0).ConfigureAwait(false);
                return accounts.First().ToAccount();
            }
        }

        public async Task PingAsync()
        {
            await SendMessageAsync(new PingMessage()).ConfigureAwait(false);
        }

        public async Task SyncFriends()
        {
            using(AccountsDatabaseContext context = new AccountsDatabaseContext()) {
                var accounts = from a in context.Accounts where a.Id == Account.Id select a;
                if(accounts.Any()) {
                    Logger.Warn($"No such account Id={Account.Id}!");
                    await SendMessageAsync(new FriendListMessage()).ConfigureAwait(false);
                    return;
                }

                AccountInfo account = accounts.First();
                Logger.Debug($"Read {account.Friends.Count} friends...");

                List<Account> friends = new List<Account>();
                foreach(AccountFriend friend in account.Friends) {
                    Account friendAccount = friend.FriendAccount.ToAccount();
                    if(null != friend.Group) {
                        friendAccount.GroupName = friend.Group.GroupName;
                    }

                    friends.Add(friendAccount);
                }

                await SendMessageAsync(new FriendListMessage()
                    {
                        Friends = friends,
                    }).ConfigureAwait(false);
            }
        }

        protected override MessagePacket CreatePacket(Message message)
        {
            return new NetworkPacket();
        }
    }
}
