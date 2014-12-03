using EnergonSoftware.Chat.Net;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Chat.MessageHandlers
{
    internal sealed class ChatMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type, Session session)
        {
            ChatSession chatSession = (ChatSession)session;
            switch(type)
            {
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
