using System;
using System.Collections.Generic;
using System.Configuration;
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
using EnergonSoftware.Database;
using EnergonSoftware.Database.Models.Accounts;

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

        protected async override Task<Account> LookupAccountAsync(string account_name)
        {
            Logger.Debug("Looking up account for account_name=" + account_name);
            AccountInfo account = new AccountInfo() { AccountName = account_name };
            using(DatabaseConnection connection = await DatabaseManager.AcquireDatabaseConnectionAsync().ConfigureAwait(false)) {
                if(!await account.ReadAsync(connection).ConfigureAwait(false)) {
                    return null;
                }
            }

            return account.ToAccount();
        }

        public async Task PingAsync()
        {
            await SendMessageAsync(new PingMessage()).ConfigureAwait(false);
        }

        public async Task SyncFriends()
        {
            List<Account> friends = new List<Account>();
            using(DatabaseConnection connection = await DatabaseManager.AcquireDatabaseConnectionAsync().ConfigureAwait(false)) {
                friends = await AccountInfo.ReadFriendsAsync(connection, Account.Id).ConfigureAwait(false);
            }

            Logger.Debug("Read " + friends.Count + " friends...");

            await SendMessageAsync(new FriendListMessage()
                {
                    Friends = friends,
                }).ConfigureAwait(false);
        }
    }
}
