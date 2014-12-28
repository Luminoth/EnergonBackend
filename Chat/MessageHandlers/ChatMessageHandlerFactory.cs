using EnergonSoftware.Chat.Net;
using EnergonSoftware.Core.MessageHandlers;

namespace EnergonSoftware.Chat.MessageHandlers
{
    internal sealed class ChatMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler Create(string type)
        {
            switch(type)
            {
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
