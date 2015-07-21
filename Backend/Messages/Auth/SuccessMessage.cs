using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
{
    /// <summary>
    /// Authentication success
    /// </summary>
    [Serializable]
    public sealed class SuccessMessage : Message
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "success";

        public override string Type => MessageType;

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public string SessionId { get; set; } = string.Empty;

        public async override Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Ticket", SessionId).ConfigureAwait(false);
        }

        public async override Task DeserializeAsync(IFormatter formatter)
        {
            SessionId = await formatter.ReadStringAsync("Ticket").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return $"SuccessMessage(SessionId={SessionId})";
        }
    }
}
