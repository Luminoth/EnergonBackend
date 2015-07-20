using System;

using EnergonSoftware.Backend.Messages.Auth;
using EnergonSoftware.Backend.Messages.Chat;
using EnergonSoftware.Backend.Messages.Network;
using EnergonSoftware.Backend.Messages.Notification;
using EnergonSoftware.Backend.Properties;

namespace EnergonSoftware.Backend.Messages
{
    /// <summary>
    /// Creates new messages
    /// </summary>
    // TODO: move this into its own file
    public interface IMessageFactory
    {
        /// <summary>
        /// Creates the a message with the specified message type.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>The message</returns>
        IMessage Create(string messageType);
    }

    /// <summary>
    /// Creates new backend messages
    /// </summary>
    public class MessageFactory : IMessageFactory
    {
        public IMessage Create(string messageType)
        {
            switch(messageType)
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

            throw new ArgumentException(string.Format(Resources.ErrorUnsupportedMessage, messageType), nameof(messageType));
        }
    }
}
