using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;
using EnergonSoftware.Backend.Messages.Formatter;

namespace EnergonSoftware.Backend.Messages.Chat
{
    [Serializable]
    public sealed class FriendListMessage : IAuthenticatedMessage
    {
        public const string MessageType = "friendlist";
        public string Type { get { return MessageType; } }

        public string AccountName { get; set; }
        public string SessionId { get; set; }

        public List<Account> Friends { get; set; }

        public FriendListMessage()
        {
            AccountName = string.Empty;
            SessionId = string.Empty;

            Friends = new List<Account>();
        }

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await formatter.WriteAsync("account_name", AccountName).ConfigureAwait(false);
            await formatter.WriteAsync("ticket", SessionId).ConfigureAwait(false);
            await formatter.WriteAsync<Account>("friendList", Friends).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            AccountName = await formatter.ReadStringAsync("account_name").ConfigureAwait(false);
            SessionId = await formatter.ReadStringAsync("sessionid").ConfigureAwait(false);
            Friends = await formatter.ReadListAsync<Account>("friendList").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "FriendListMessage()";
        }
    }
}
