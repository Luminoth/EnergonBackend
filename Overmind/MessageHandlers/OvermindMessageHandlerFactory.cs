using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Chat;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class OvermindMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler Create(string type)
        {
            switch(type)
            {
            case PingMessage.MessageType:
                return new PingMessageHandler();
            case LogoutMessage.MessageType:
                return new LogoutMessageHandler();
            case VisibilityMessage.MessageType:
                return new VisibilityMessageHandler();
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
