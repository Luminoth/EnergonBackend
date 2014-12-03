using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Chat
{
    public sealed class OnlineMessage : IMessage
    {
        public const string MessageType = "online";
        public string Type { get { return MessageType; } }

        public OnlineMessage()
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
            return "OnlineMessage()";
        }
    }
}
