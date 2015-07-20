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
    public sealed class FriendListMessage : IAuthenticatedMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "friendlist";

        public string Type { get { return MessageType; } }

        public string AccountName { get; set; }

        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the friends.
        /// </summary>
        /// <value>
        /// The friends.
        /// </value>
        public List<Account> Friends { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FriendListMessage"/> class.
        /// </summary>
        public FriendListMessage()
        {
            AccountName = string.Empty;
            SessionId = string.Empty;

            Friends = new List<Account>();
        }

        public async Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("AccountName", AccountName).ConfigureAwait(false);
            await formatter.WriteAsync("Ticket", SessionId).ConfigureAwait(false);
            await formatter.WriteAsync("FriendList", Friends).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
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
