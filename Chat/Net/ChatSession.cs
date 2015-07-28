using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;
using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Chat;
using EnergonSoftware.Backend.Net.Sessions;
using EnergonSoftware.Backend.Packet;

using EnergonSoftware.Chat.MessageHandlers;

using EnergonSoftware.Core.Serialization.Formatters;

using EnergonSoftware.DAL;
using EnergonSoftware.DAL.Models.Accounts;

using log4net;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class ChatSession : AuthenticatedNetworkSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatSession));

        public override string Name => "chat";

        // TODO: make this configurable
        public override int MaxSessionReceiveBufferSize => 1024 * 1000 * 10;

        protected override string MessageFormatterType => BinaryNetworkFormatter.FormatterType;

        protected override string PacketType => NetworkPacket.PacketType;

        public override IMessageHandlerFactory MessageHandlerFactory => new ChatMessageHandlerFactory();

        public ChatSession(Socket socket)
            : base(socket)
        {
            MessageReceivedEvent += Chat.Instance.MessageProcessor.MessageReceivedEventHandler;
        }

        protected async override Task<Account> LookupAccountAsync(string accountName)
        {
            Logger.Debug($"Looking up account for accountName={accountName}");
            using(AccountsDatabaseContext context = new AccountsDatabaseContext()) {
                var accounts = from a in context.Accounts where a.AccountName == accountName select a;
                if(!accounts.Any()) {
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

        public async Task SyncFriends()
        {
            using(AccountsDatabaseContext context = new AccountsDatabaseContext()) {
                var accounts = from a in context.Accounts where a.Id == Account.Id select a;
                if(!accounts.Any()) {
                    Logger.Warn($"No such account Id={Account.Id}!");
                    await SendAsync(new FriendListMessage()).ConfigureAwait(false);
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

                await SendAsync(new FriendListMessage()
                    {
                        Friends = friends,
                    }).ConfigureAwait(false);
            }
        }
    }
}
