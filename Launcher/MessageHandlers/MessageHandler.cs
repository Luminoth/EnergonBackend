using System.Collections.Generic;
using System.Threading;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;

using EnergonSoftware.Launcher.MessageHandlers.Auth;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class MessageHandlerContext
    {
        private volatile IMessage _message;

        public IMessage Message { get { return _message; } }

        public MessageHandlerContext(IMessage message)
        {
            _message = message;
        }
    }

    internal interface IMessageHandler
    {
        void HandleMessage(object context);
    }

    internal static class MessageHandler
    {
        private static Dictionary<string, IMessageHandler> Handlers = new Dictionary<string,IMessageHandler>();

        static MessageHandler()
        {
            Handlers[ChallengeMessage.MESSAGE_TYPE] = new ChallengeMessageHandler();
            Handlers[FailureMessage.MESSAGE_TYPE] = new FailureMessageHandler();
            Handlers[SuccessMessage.MESSAGE_TYPE] = new SuccessMessageHandler();
        }

        internal static void HandleMessage(IMessage message)
        {
            IMessageHandler handler = null;
            try {
                handler = Handlers[message.Type];
            } catch(KeyNotFoundException) {
                throw new MessageHandlerException("Unsupported message type: " + message.Type);
            }
            ThreadPool.QueueUserWorkItem(handler.HandleMessage, new MessageHandlerContext(message));
        }
    }
}
