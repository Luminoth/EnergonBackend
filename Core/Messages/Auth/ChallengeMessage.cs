using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
{
    public sealed class ChallengeMessage : IMessage
    {
        public string Type { get { return "challenge"; } }

        public string Challenge = "";

        public ChallengeMessage()
        {
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteString(Challenge, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            Challenge = formatter.ReadString(stream);
        }
    }
}
