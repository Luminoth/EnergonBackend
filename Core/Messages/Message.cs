using System.IO;
using System.Runtime.Serialization;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages
{
    public interface IMessage : IMessageSerializable
    {
        string Type { get; }
    }

    public static class MessageFactory
    {
        public static IMessage CreateMessage(string type)
        {
            switch(type)
            {
            default:
                return null;
            }
            throw new MessageException("Unsupported message type: " + type);
        }
    }
}
