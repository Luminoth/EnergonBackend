using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Chat
{
    [Serializable]
    public sealed class FriendListMessage : IAuthenticatedMessage
    {
        public const string MessageType = "friendlist";
        public string Type { get { return MessageType; } }

        public string Username { get; set; }
        public string SessionId { get; set; }

        public List<Account> Friends { get; set; }

        public FriendListMessage()
        {
            Username = string.Empty;
            SessionId = string.Empty;

            Friends = new List<Account>();
        }

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await formatter.WriteAsync("username", Username).ConfigureAwait(false);
            await formatter.WriteAsync("ticket", SessionId).ConfigureAwait(false);
            await formatter.WriteAsync<Account>("friends", Friends).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            Username = await formatter.ReadStringAsync("username").ConfigureAwait(false);
            SessionId = await formatter.ReadStringAsync("sessionid").ConfigureAwait(false);
            Friends = await formatter.ReadListAsync<Account>("friends").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "FriendListMessage()";
        }
    }
}
