using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Chat;

using EnergonSoftware.Chat.Net;

using EnergonSoftware.Core.Messages;

namespace EnergonSoftware.Chat.MessageHandlers
{
    internal sealed class ChatMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler Create(string type)
        {
            switch(type)
            {
            case PingMessage.MessageType:
                return new PingMessageHandler();
            case LoginMessage.MessageType:
                return new LoginMessageHandler();
            case LogoutMessage.MessageType:
                return new LogoutMessageHandler();
            case VisibilityMessage.MessageType:
                return new VisibilityMessageHandler();
            }

            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
