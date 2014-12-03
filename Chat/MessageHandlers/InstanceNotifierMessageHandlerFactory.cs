﻿using EnergonSoftware.Chat.Net;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Notifications;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Chat.MessageHandlers
{
    internal sealed class InstanceNotifierMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type, Session session)
        {
            InstanceNotifierSession notifierSession = (InstanceNotifierSession)session;
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