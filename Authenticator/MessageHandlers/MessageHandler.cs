using System;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Authenticator.Net;

namespace EnergonSoftware.Authenticator.MessageHandlers
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

    internal abstract class MessageHandler
    {
        public static MessageHandler Create(string type)
        {
            switch(type)
            {
            case AuthMessage.MESSAGE_TYPE:
                return new AuthMessageHandler();
            case ResponseMessage.MESSAGE_TYPE:
                return new ResponseMessageHandler();
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }

        public bool Finished { get; set; }

        public void HandleMessage(object context)
        {
            MessageHandlerContext ctx = (MessageHandlerContext)context;

            Finished = false;
            try {
                OnHandleMessage(context);
            } catch(Exception e) {
                ctx.Session.Error(e);
            }
            Finished = true;
        }

        protected abstract void OnHandleMessage(object context);
    }
}
