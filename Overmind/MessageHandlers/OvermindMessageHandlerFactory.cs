﻿using System;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class OvermindMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler Create(string type)
        {
            switch(type)
            {
            case PingMessage.MessageType:
                return new PingMessageHandler();
            case LoginMessage.MessageType:
                return new LoginMessageHandler();
            case LogoutMessage.MessageType:
                return new LogoutMessageHandler();
            }

            throw new ArgumentException("Unsupported message type", nameof(type));
        }
    }
}
