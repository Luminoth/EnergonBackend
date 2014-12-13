using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Launcher.MessageHandlers.Auth;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class MessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type)
        {
            switch(type)
            {
            case PingMessage.MessageType:
                return new PingMessageHandler();
            case LogoutMessage.MessageType:
                return new LogoutMessageHandler();
            case ChallengeMessage.MessageType:
                return new ChallengeMessageHandler();
            case FailureMessage.MessageType:
                return new FailureMessageHandler();
            case SuccessMessage.MessageType:
                return new SuccessMessageHandler();
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
