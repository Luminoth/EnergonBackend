using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class InstanceNotifierMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type, Session session)
        {
            InstanceNotifierSession loginSession = (InstanceNotifierSession)session;
            switch(type)
            {
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
