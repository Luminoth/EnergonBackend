using System.IO;
using System.Runtime.Serialization;

using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Notifications;
using EnergonSoftware.Core.Messages.Overmind;

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
