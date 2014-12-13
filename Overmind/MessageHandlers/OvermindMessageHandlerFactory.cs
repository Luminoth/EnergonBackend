using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class OvermindMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type)
        {
            switch(type)
            {
            case PingMessage.MessageType:
                return new PingMessageHandler();
            case LoginMessage.MessageType:
                return new LoginMessageHandler();
            case LogoutMessage.MessageType:
                return new LogoutMessageHandler();
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
