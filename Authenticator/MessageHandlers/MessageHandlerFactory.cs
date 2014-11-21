using EnergonSoftware.Authenticator.Net;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    internal sealed class MessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type, Session session)
        {
            AuthSession authSession = (AuthSession)session;
            switch(type)
            {
            case AuthMessage.MessageType:
                return new AuthMessageHandler(authSession);
            case ResponseMessage.MessageType:
                return new ResponseMessageHandler(authSession);
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
