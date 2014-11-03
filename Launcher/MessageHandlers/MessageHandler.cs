using System;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;

using EnergonSoftware.Launcher.MessageHandlers.Auth;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class MessageHandlerContext
    {
        public IMessage Message { get; private set; }

        public MessageHandlerContext(IMessage message)
        {
            Message = message;
        }
    }

    internal abstract class MessageHandler
    {
        public static MessageHandler Create(string type)
        {
            switch(type)
            {
            case PingMessage.MESSAGE_TYPE:
                return new PingMessageHandler();
            case ChallengeMessage.MESSAGE_TYPE:
                return new ChallengeMessageHandler();
            case FailureMessage.MESSAGE_TYPE:
                return new FailureMessageHandler();
            case SuccessMessage.MESSAGE_TYPE:
                return new SuccessMessageHandler();
            }
            throw new MessageHandlerException("Unsupported message type: " + type);
        }

        public bool Finished { get; set; }

        public void HandleMessage(object context)
        {
            Finished = false;
            OnHandleMessage(context);
            Finished = true;
        }

        protected abstract void OnHandleMessage(object context);
    }
}
