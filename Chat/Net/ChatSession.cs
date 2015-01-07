using System;
using System.Configuration;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Chat.MessageHandlers;
using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Models;

using log4net;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class ChatSessionFactory : ISessionFactory
    {
        public Session Create(Socket socket)
        {
            return new ChatSession(socket)
            {
                Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]),
            };
        }
    }

    internal sealed class ChatSession : AuthenticatedSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatSession));

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        protected override IMessageHandlerFactory HandlerFactory { get { return new ChatMessageHandlerFactory(); } }

        public ChatSession(Socket socket) : base(socket)
        {
        }

        protected async override Task<Account> LookupAccountAsync(string username)
        {
            Logger.Debug("Looking up account for username=" + username);
            AccountInfo account = new AccountInfo() { Username = username };
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
    }
}
