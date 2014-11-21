using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages
{
    public sealed class PingMessage : IMessage
    {
        public const string MessageType = "ping";
        public string Type { get { return MessageType; } }

        public PingMessage()
        {
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
        }

        public override string ToString()
        {
            return "PingMessage()";
        }
    }
}
