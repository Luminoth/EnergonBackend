﻿using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Notification;
using EnergonSoftware.Core.Net;

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
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
