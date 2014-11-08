using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Launcher.MessageHandlers.Auth;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    sealed class MessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type, Session session)
        {
            switch(type)
            {
            case PingMessage.MESSAGE_TYPE:
                return new PingMessageHandler();
            case LogoutMessage.MESSAGE_TYPE:
                return new LogoutMessageHandler((OvermindSession)session);
            case ChallengeMessage.MESSAGE_TYPE:
                return new ChallengeMessageHandler((AuthSession)session);
            case FailureMessage.MESSAGE_TYPE:
                return new FailureMessageHandler((AuthSession)session);
            case SuccessMessage.MESSAGE_TYPE:
                return new SuccessMessageHandler((AuthSession)session);
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
