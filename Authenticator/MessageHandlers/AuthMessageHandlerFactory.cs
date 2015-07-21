using System;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages.Auth;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    internal sealed class AuthMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler Create(string type)
        {
            switch(type)
            {
            case AuthMessage.MessageType:
                return new AuthMessageHandler();
            case ResponseMessage.MessageType:
                return new ResponseMessageHandler();
            }

            throw new ArgumentException("Unsupported message type", nameof(type));
        }
    }
}
