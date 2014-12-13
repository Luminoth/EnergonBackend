using System;
using System.IO;
using System.Runtime.Serialization;

using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Notification;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages
{
    public interface IMessage : IMessageSerializable
    {
        string Type { get; }
    }

    public abstract class MessagePacket : IComparable
    {
#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

        public int Id { get; protected set; }

        public IMessage Payload { get; set; }
        public bool HasPayload { get { return null != Payload; } }

        public MessagePacket()
        {
            Id = NextId;
        }

        public abstract byte[] Serialize(IMessageFormatter formatter);
        public abstract bool DeSerialize(MemoryBuffer buffer, IMessageFormatter formatter);

        public int CompareTo(object obj)
        {
            MessagePacket rhs = obj as MessagePacket;
            if(null == rhs) {
                return 0;
            }
            return (int)(Id - rhs.Id);
        }
    }

    public static class MessageFactory
    {
        public static IMessage CreateMessage(string type)
        {
            switch(type)
            {
            case "null":
                return null;

            /* notifications */
            case StartupMessage.MessageType:
                return new StartupMessage();
            case ShutdownMessage.MessageType:
                return new ShutdownMessage();

            /* misc */
            case PingMessage.MessageType:
                return new PingMessage();

            /* auth */
            case AuthMessage.MessageType:
                return new AuthMessage();
            case ChallengeMessage.MessageType:
                return new ChallengeMessage();
            case FailureMessage.MessageType:
                return new FailureMessage();
            case ResponseMessage.MessageType:
                return new ResponseMessage();
            case SuccessMessage.MessageType:
                return new SuccessMessage();

            /* overmind */
            case LoginMessage.MessageType:
                return new LoginMessage();
            case LogoutMessage.MessageType:
                return new LogoutMessage();
            }
            throw new MessageException("Unsupported message type for construction: " + type);
        }
    }
}
