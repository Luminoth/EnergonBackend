using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Chat;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Network;
using EnergonSoftware.Core.Messages.Notification;
using EnergonSoftware.Core.Properties;

namespace EnergonSoftware.Core.Messages
{
    public interface IMessage : IMessageSerializable
    {
    }

    public static class MessageFactory
    {
        public static IMessage Create(string type)
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
            case LoginMessage.MessageType:
                return new LoginMessage();
            case LogoutMessage.MessageType:
                return new LogoutMessage();

            /* network */
            case StartTLSMessage.MessageType:
                return new StartTLSMessage();

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

            /* chat */
            case FriendListMessage.MessageType:
                return new FriendListMessage();
            case VisibilityMessage.MessageType:
                return new VisibilityMessage();
            }

            throw new MessageException(string.Format(Resources.ErrorUnsupportedMessage, type));
        }
    }
}
