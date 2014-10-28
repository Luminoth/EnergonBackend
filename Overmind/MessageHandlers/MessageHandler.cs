using System.Collections.Generic;
using System.Threading;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class MessageHandlerContext
    {
        public Session Session { get; private set; }
        public IMessage Message { get; private set; }

        public MessageHandlerContext(Session session, IMessage message)
        {
            Session = session;
            Message = message;
        }
    }

    interface IMessageHandler
    {
        void HandleMessage(object context);
    }

    static class MessageHandler
    {
        public static Dictionary<string, IMessageHandler> Handlers = new Dictionary<string,IMessageHandler>();

        static MessageHandler()
        {
        }

        public static void HandleMessage(IMessage message, Session session)
        {
            IMessageHandler handler = null;
            try {
                handler = Handlers[message.Type];
            } catch(KeyNotFoundException) {
                throw new MessageHandlerException("Unsupported message type: " + message.Type);
            }
            ThreadPool.QueueUserWorkItem(handler.HandleMessage, new MessageHandlerContext(session, message));
        }
    }
}
