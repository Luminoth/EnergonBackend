using System;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;
using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Parser;
using EnergonSoftware.Backend.Net.Sessions;

using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.DAL;

using EnergonSoftware.Overmind.MessageHandlers;

using log4net;

namespace EnergonSoftware.Overmind.Net
{
    internal sealed class OvermindSessionFactory : INetworkSessionFactory
    {
        public NetworkSession Create(Socket socket)
        {
            OvermindSession session = null;
            try {
                session = new OvermindSession(socket)
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

    internal sealed class OvermindSession : AuthenticatedSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OvermindSession));

        public override string Name => "overmind";

        private readonly NetworkPacketParser _messageParser = new NetworkPacketParser();
        private readonly MessageProcessor _messageProcessor = new MessageProcessor();
        private readonly IMessageHandlerFactory _messageHandlerFactory = new OvermindMessageHandlerFactory();

        protected override string FormatterType => BinaryMessageFormatter.FormatterType;

        public OvermindSession(Socket socket) : base(socket)
        {
        }

        protected async override Task<Account> LookupAccountAsync(string accountName)
        {
            Logger.Debug("Looking up account for accountName=" + accountName);
            using(AccountsDatabaseContext context = new AccountsDatabaseContext()) {
                var accounts = from a in context.Accounts where a.AccountName == accountName select a;
                if(accounts.Any()) {
                    return null;
                }

                await Task.Delay(0).ConfigureAwait(false);
                return accounts.First().ToAccount();
            }
        }

        protected override MessagePacket CreatePacket(IMessage message)
        {
            return new NetworkPacket();
        }

        public async Task PingAsync()
        {
            await SendMessageAsync(new PingMessage()).ConfigureAwait(false);
        }
    }
}
