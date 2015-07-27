using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;
using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Net.Sessions;
using EnergonSoftware.Backend.Packet;
using EnergonSoftware.Core.Serialization.Formatters;
using EnergonSoftware.DAL;

using EnergonSoftware.Overmind.MessageHandlers;

using log4net;

namespace EnergonSoftware.Overmind.Net
{
    internal sealed class OvermindSession : AuthenticatedNetworkSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OvermindSession));

        public override string Name => "overmind";

        // TODO: make this configurable
        public override int MaxSessionReceiveBufferSize => 1024 * 1000 * 10;

        protected override string MessageFormatterType => BinaryNetworkFormatter.FormatterType;

        protected override string PacketType => NetworkPacket.PacketType;

        public override IMessageHandlerFactory MessageHandlerFactory => new OvermindMessageHandlerFactory();

        public OvermindSession(Socket socket)
            : base(socket)
        {
            MessageReceivedEvent += Overmind.Instance.MessageProcessor.MessageReceivedEventHandler;
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
            await SendAsync(new PingMessage()).ConfigureAwait(false);
        }
    }
}
