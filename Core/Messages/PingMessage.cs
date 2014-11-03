using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages
{
    public sealed class PingMessage : IMessage
    {
        public const string MESSAGE_TYPE = "ping";
        public string Type { get { return MESSAGE_TYPE; } }

        public PingMessage()
        {
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
        }
    }
}
