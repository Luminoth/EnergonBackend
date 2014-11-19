using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;

namespace EnergonSoftware.Core.Test.MessageHandlers
{
    sealed class ExceptionMessageHandler : MessageHandler
    {
        internal ExceptionMessageHandler()
        {
        }

        protected override Task OnHandleMessage(IMessage message)
        {
            throw new MessageHandlerException("Test Exception");
        }
    }
}
