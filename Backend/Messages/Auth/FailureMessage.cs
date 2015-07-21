using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
{
    /// <summary>
    /// Authentication failure
    /// </summary>
    [Serializable]
    public sealed class FailureMessage : Message
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "failure";

        public override string Type => MessageType;

        /// <summary>
        /// Gets or sets the failure reason.
        /// </summary>
        /// <value>
        /// The failure reason.
        /// </value>
        public string Reason { get; set; } = string.Empty;

        public async override Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Reason", Reason).ConfigureAwait(false);
        }

        public async override Task DeserializeAsync(IFormatter formatter)
        {
            Reason = await formatter.ReadStringAsync("Reason").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return $"FailureMessage(Reason={Reason})";
        }
    }
}
