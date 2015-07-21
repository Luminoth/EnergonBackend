using System;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Chat
{
    /// <summary>
    /// Account visibility
    /// </summary>
    [Serializable]
    public sealed class VisibilityMessage : AuthenticatedMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "visibility";

        public override string Type => MessageType;

        public override string AccountName { get; set; } = string.Empty;

        public override string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the visibility.
        /// </summary>
        /// <value>
        /// The visibility.
        /// </value>
        public Visibility Visibility { get; set; } = Visibility.Online;

        public async override Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("AccountName", AccountName).ConfigureAwait(false);
            await formatter.WriteAsync("Ticket", SessionId).ConfigureAwait(false);
            await formatter.WriteAsync("Visibility", (int)Visibility).ConfigureAwait(false);
        }

        public async override Task DeserializeAsync(IFormatter formatter)
        {
            AccountName = await formatter.ReadStringAsync("AccountName").ConfigureAwait(false);
            SessionId = await formatter.ReadStringAsync("Ticket").ConfigureAwait(false);
            Visibility = (Visibility)await formatter.ReadIntAsync("Visibility").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return $"VisibilityMessage(Visibility={Visibility})";
        }
    }
}
