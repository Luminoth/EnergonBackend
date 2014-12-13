using System;
using System.IO;

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

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteString(Challenge, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            Challenge = formatter.ReadString(stream);
        }

        public override string ToString()
        {
            return "ChallengeMessage(Challenge=" + Challenge + ")";
        }
    }
}
