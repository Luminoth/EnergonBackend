using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Notification;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Manager.MessageHandlers
{
    internal sealed class InstanceNotifierMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type)
        {
            switch(type)
            {
            case StartupMessage.MessageType:
                return new MessageHandler();
            case ShutdownMessage.MessageType:
                return new MessageHandler();
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
