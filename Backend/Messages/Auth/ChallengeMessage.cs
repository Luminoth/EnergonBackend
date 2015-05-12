using System;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages.Formatter;

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

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await formatter.WriteAsync("challenge", Challenge).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            Challenge = await formatter.ReadStringAsync("challenge").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "ChallengeMessage(Challenge=" + Challenge + ")";
        }
    }
}
