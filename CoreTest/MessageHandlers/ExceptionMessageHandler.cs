using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;

namespace EnergonSoftware.Core.Test.MessageHandlers
{
    internal sealed class ExceptionMessageHandler : MessageHandler
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
