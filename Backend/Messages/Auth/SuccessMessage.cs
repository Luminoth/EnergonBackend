using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
{
    /// <summary>
    /// Authentication success
    /// </summary>
    [Serializable]
    public sealed class SuccessMessage : IMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "success";

        public string Type { get { return MessageType; } }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public string SessionId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuccessMessage"/> class.
        /// </summary>
        public SuccessMessage()
        {
            SessionId = string.Empty;
        }

        public async Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Ticket", SessionId).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
        {
            SessionId = await formatter.ReadStringAsync("Ticket").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "SuccessMessage(SessionId=" + SessionId + ")";
        }
    }
}
