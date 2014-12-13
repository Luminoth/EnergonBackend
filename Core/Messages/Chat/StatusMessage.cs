using System;
using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Chat
{
    [Serializable]
    public sealed class StatusMessage : IMessage
    {
        public const string MessageType = "status";
        public string Type { get { return MessageType; } }

        public StatusMessage()
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
