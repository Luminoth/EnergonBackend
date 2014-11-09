using System.IO;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Test.Messages
{
    public sealed class ExceptionMessage : IMessage
    {
        public const string MESSAGE_TYPE = "exception";
        public string Type { get { return MESSAGE_TYPE; } }

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
