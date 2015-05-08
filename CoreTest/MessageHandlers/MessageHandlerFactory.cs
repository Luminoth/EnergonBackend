using EnergonSoftware.Backend.MessageHandlers;

using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Test.Messages;

namespace EnergonSoftware.Core.Test.MessageHandlers
{
    internal sealed class MessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler Create(string type)
        {
            switch(type)
            {
            case ExceptionMessage.MessageType:
                return new ExceptionMessageHandler();
            }

            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
