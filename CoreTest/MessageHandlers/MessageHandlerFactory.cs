using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Test.Messages;

namespace EnergonSoftware.Core.Test.MessageHandlers
{
    sealed class MessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type, Session session)
        {
            switch(type)
            {
            case ExceptionMessage.MESSAGE_TYPE:
                return new ExceptionMessageHandler();
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
