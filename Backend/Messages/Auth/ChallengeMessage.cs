using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
{
    /// <summary>
    /// Authentication challenge
    /// </summary>
    [Serializable]
    public sealed class ChallengeMessage : IMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "challenge";

        public string Type { get { return MessageType; } }

        /// <summary>
        /// Gets or sets the challenge.
        /// </summary>
        /// <value>
        /// The challenge.
        /// </value>
        public string Challenge { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChallengeMessage"/> class.
        /// </summary>
        public ChallengeMessage()
        {
            Challenge = string.Empty;
        }

        public async Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Challenge", Challenge).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
        {
            Challenge = await formatter.ReadStringAsync("Challenge").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "ChallengeMessage(Challenge=" + Challenge + ")";
        }
    }
}
