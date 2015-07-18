using EnergonSoftware.Backend.MessageHandlers;

using EnergonSoftware.Backend.Test.Messages;

namespace EnergonSoftware.Backend.UnitTests.MessageHandlers
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
