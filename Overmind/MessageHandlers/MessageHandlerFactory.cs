using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    sealed class MessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type, Session session)
        {
            LoginSession loginSession = (LoginSession)session;
            switch(type)
            {
            case PingMessage.MESSAGE_TYPE:
                return new PingMessageHandler(loginSession);
            case LoginMessage.MESSAGE_TYPE:
                return new LoginMessageHandler(loginSession);
            case LogoutMessage.MESSAGE_TYPE:
                return new LogoutMessageHandler(loginSession);
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
