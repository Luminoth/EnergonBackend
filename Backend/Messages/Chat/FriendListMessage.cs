using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Chat
{
    /// <summary>
    /// Friend list
    /// </summary>
    [Serializable]
    public sealed class FriendListMessage : AuthenticatedMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "friendlist";

        public override string Type => MessageType;

        public override string AccountName { get; set; } = string.Empty;

        public override string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the friends.
        /// </summary>
        /// <value>
        /// The friends.
        /// </value>
        public IReadOnlyCollection<Account> Friends { get; set; } = new List<Account>();

        public async override Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("AccountName", AccountName).ConfigureAwait(false);
            await formatter.WriteAsync("Ticket", SessionId).ConfigureAwait(false);
            await formatter.WriteAsync("FriendList", Friends).ConfigureAwait(false);
        }

        public async override Task DeserializeAsync(IFormatter formatter)
        {
            AccountName = await formatter.ReadStringAsync("AccountName").ConfigureAwait(false);
            SessionId = await formatter.ReadStringAsync("Ticket").ConfigureAwait(false);
            Friends = await formatter.ReadListAsync<Account>("FriendList").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "FriendListMessage()";
        }
    }
}
