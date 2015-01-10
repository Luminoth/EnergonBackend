using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task SerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            await formatter.WriteStringAsync(Username, stream).ConfigureAwait(false);
            await formatter.WriteStringAsync(SessionId, stream).ConfigureAwait(false);
            await formatter.WriteListAsync<Account>(Friends, stream).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            Username = await formatter.ReadStringAsync(stream).ConfigureAwait(false);
            SessionId = await formatter.ReadStringAsync(stream).ConfigureAwait(false);
            Friends = await formatter.ReadListAsync<Account>(stream).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "FriendListMessage()";
        }
    }
}
