using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    internal sealed class AuthMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type)
        {
            switch(type)
            {
            case AuthMessage.MessageType:
                return new AuthMessageHandler();
            case ResponseMessage.MessageType:
                return new ResponseMessageHandler();
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
