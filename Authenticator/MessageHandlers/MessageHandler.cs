using System;
using System.Collections.Generic;
using System.Threading;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Authenticator.Net;

namespace EnergonSoftware.Authenticator.MessageHandlers
{
    internal sealed class MessageHandlerContext
    {
        private volatile Session _session;
        private volatile IMessage _message;

        public Session Session { get { return _session; } }
        public IMessage Message { get { return _message; } }

        public MessageHandlerContext(Session session, IMessage message)
        {
            _session = session;
            _message = message;
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
            Handlers["auth"] = new AuthMessageHandler();
            Handlers["response"] = new ResponseMessageHandler();
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
