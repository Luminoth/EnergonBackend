using System.IO;
using System.Runtime.Serialization;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages
{
    public enum MessageType
    {
        None,
    }

    public interface IMessage : IMessageSerializable
    {
        MessageType Type { get; }
    }

    public static class MessageFactory
    {
        public static IMessage CreateMessage(MessageType type)
        {
            switch(type)
            {
            case MessageType.None:
                return null;
            }
            throw new MessageException("Unsupported message type: " + type);
        }
    }
}
