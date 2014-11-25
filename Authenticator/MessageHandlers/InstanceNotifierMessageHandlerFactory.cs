using EnergonSoftware.Authenticator.Net;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    internal sealed class InstanceNotifierMessageHandlerFactory : IMessageHandlerFactory
    {
        public MessageHandler NewHandler(string type, Session session)
        {
            InstanceNotifierSession authSession = (InstanceNotifierSession)session;
            switch(type)
            {
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }
    }
}
