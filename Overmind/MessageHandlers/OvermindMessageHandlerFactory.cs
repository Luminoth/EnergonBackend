using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class OvermindMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type, Session session)
        {
            LoginSession loginSession = (LoginSession)session;
            switch(type)
            {
            case PingMessage.MessageType:
                return new PingMessageHandler(loginSession);
            case LoginMessage.MessageType:
                return new LoginMessageHandler(loginSession);
            case LogoutMessage.MessageType:
                return new LogoutMessageHandler(loginSession);
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
