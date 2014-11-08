using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Authenticator.Net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    sealed class MessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type, Session session)
        {
            AuthSession authSession = (AuthSession)session;
            switch(type)
            {
            case AuthMessage.MESSAGE_TYPE:
                return new AuthMessageHandler(authSession);
            case ResponseMessage.MESSAGE_TYPE:
                return new ResponseMessageHandler(authSession);
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
