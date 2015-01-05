using System;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
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

        public async Task SerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            await formatter.WriteStringAsync(Challenge, stream).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            Challenge = await formatter.ReadStringAsync(stream).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "ChallengeMessage(Challenge=" + Challenge + ")";
        }
    }
}
