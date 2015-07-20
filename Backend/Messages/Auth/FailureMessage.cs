using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
{
    /// <summary>
    /// Authentication failure
    /// </summary>
    [Serializable]
    public sealed class FailureMessage : IMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "failure";

        public string Type { get { return MessageType; } }

        /// <summary>
        /// Gets or sets the failure reason.
        /// </summary>
        /// <value>
        /// The failure reason.
        /// </value>
        public string Reason { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailureMessage"/> class.
        /// </summary>
        public FailureMessage()
        {
            Reason = string.Empty;
        }

        public async Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Reason", Reason).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
        {
            Reason = await formatter.ReadStringAsync("Reason").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "FailureMessage(Reason=" + Reason + ")";
        }
    }
}
