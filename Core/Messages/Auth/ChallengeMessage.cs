using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
{
    public sealed class ChallengeMessage : IMessage
    {
        public const string MESSAGE_TYPE = "challenge";
        public string Type { get { return MESSAGE_TYPE; } }

        public string Challenge { get; set; }

        public ChallengeMessage()
        {
            Challenge = "";
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
