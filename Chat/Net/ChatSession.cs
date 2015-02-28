using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Chat.MessageHandlers;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Chat;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.DAL;
using EnergonSoftware.DAL.Models.Accounts;

using log4net;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class ChatSessionFactory : ISessionFactory
    {
        public Session Create(Socket socket)
        {
            ChatSession session = null;
            try {
                session = new ChatSession(socket);
                session.Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
                return session;
            } catch(Exception) {
                if(null != session) {
                    session.Dispose();
                }

                throw;
            }
        }
    }

    internal sealed class ChatSession : AuthenticatedSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatSession));

        public override string Name { get { return "chat"; } }

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override string FormatterType { get { return BinaryMessageFormatter.FormatterType; } }
        protected override IMessageHandlerFactory HandlerFactory { get { return new ChatMessageHandlerFactory(); } }

        public ChatSession(Socket socket) : base(socket)
        {
        }

        protected async override Task<Account> LookupAccountAsync(string accountName)
        {
            Logger.Debug("Looking up account for accountName=" + accountName);
            using(AccountsDatabaseContext context = new AccountsDatabaseContext()) {
                var accounts = from a in context.Accounts where a.AccountName == accountName select a;
                if(accounts.Count() < 1) {
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
                if(accounts.Count() < 1) {
                    Logger.Warn("No such account Id=" + Account.Id + "!");
                    await SendMessageAsync(new FriendListMessage()).ConfigureAwait(false);
                    return;
                }

                AccountInfo account = accounts.First();
                Logger.Debug("Read " + account.Friends.Count + " friends...");

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
    }
}
