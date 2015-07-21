using System;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;
using EnergonSoftware.Backend.Messages.Chat;

namespace EnergonSoftware.Chat.MessageHandlers
{
    internal sealed class ChatMessageHandlerFactory : IMessageHandlerFactory
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
            case VisibilityMessage.MessageType:
                return new VisibilityMessageHandler();
            }

            throw new ArgumentException("Unsupported message type", nameof(type));
        }
    }
}
