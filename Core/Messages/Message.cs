using System.IO;
using System.Runtime.Serialization;

using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Overmind;
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
            case "null":
                return null;
            case PingMessage.MESSAGE_TYPE:
                return new PingMessage();
            case AuthMessage.MESSAGE_TYPE:
                return new AuthMessage();
            case ChallengeMessage.MESSAGE_TYPE:
                return new ChallengeMessage();
            case FailureMessage.MESSAGE_TYPE:
                return new FailureMessage();
            case ResponseMessage.MESSAGE_TYPE:
                return new ResponseMessage();
            case SuccessMessage.MESSAGE_TYPE:
                return new SuccessMessage();
            case LoginMessage.MESSAGE_TYPE:
                return new LoginMessage();
            case LogoutMessage.MESSAGE_TYPE:
                return new LogoutMessage();
            }
            throw new MessageException("Unsupported message type for construction: " + type);
        }
    }
}
