using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
{
    [Serializable]
    public sealed class ChallengeMessage : IMessage
    {
        public const string MessageType = "challenge";
        public string Type { get { return MessageType; } }

        public string Challenge { get; set; }

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
