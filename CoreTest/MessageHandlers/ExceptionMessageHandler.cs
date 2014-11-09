using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;

namespace EnergonSoftware.Core.Test.MessageHandlers
{
    sealed class ExceptionMessageHandler : MessageHandler
    {
        internal ExceptionMessageHandler()
        {
        }

        protected override void OnHandleMessage(IMessage message)
        {
            throw new MessageHandlerException("Test Exception");
        }
    }
}
