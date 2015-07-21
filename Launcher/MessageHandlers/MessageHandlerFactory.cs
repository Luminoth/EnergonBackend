using System;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;
using EnergonSoftware.Backend.Messages.Chat;

using EnergonSoftware.Launcher.MessageHandlers.Auth;
using EnergonSoftware.Launcher.MessageHandlers.Chat;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class MessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler Create(string type)
        {
            switch(type)
            {
            /* misc */
            case PingMessage.MessageType:
                return new PingMessageHandler();
            case LoginMessage.MessageType:
                return new LoginMessageHandler();
            case LogoutMessage.MessageType:
                return new LogoutMessageHandler();

            /* auth */
            case ChallengeMessage.MessageType:
                return new ChallengeMessageHandler();
            case FailureMessage.MessageType:
                return new FailureMessageHandler();
            case SuccessMessage.MessageType:
                return new SuccessMessageHandler();

            /* chat */
            case FriendListMessage.MessageType:
                return new FriendListMessageHandler();
            }

            throw new ArgumentException("Unsupported message type", nameof(type));
        }
    }
}
