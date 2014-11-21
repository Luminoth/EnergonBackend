using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Launcher.MessageHandlers.Auth;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class MessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type, Session session)
        {
            switch(type)
            {
            case PingMessage.MessageType:
                return new PingMessageHandler();
            case LogoutMessage.MessageType:
                return new LogoutMessageHandler((OvermindSession)session);
            case ChallengeMessage.MessageType:
                return new ChallengeMessageHandler((AuthSession)session);
            case FailureMessage.MessageType:
                return new FailureMessageHandler((AuthSession)session);
            case SuccessMessage.MessageType:
                return new SuccessMessageHandler((AuthSession)session);
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
