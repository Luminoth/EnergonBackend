using System.IO;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Test.Messages
{
    public sealed class ExceptionMessage : IMessage
    {
        public const string MessageType = "exception";
        public string Type { get { return MessageType; } }

        public ExceptionMessage()
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
