using System;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages.Notification;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class InstanceNotifierMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler Create(string type)
        {
            switch(type)
            {
            case StartupMessage.MessageType:
                return new MessageHandler();
            case ShutdownMessage.MessageType:
                return new MessageHandler();
            }

            throw new ArgumentException("Unsupported message type", nameof(type));
        }
    }
}
