using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Core.Test.MessageHandlers
{
    internal sealed class ExceptionMessageHandler : MessageHandler
    {
        internal ExceptionMessageHandler()
        {
        }

        protected override void OnHandleMessage(IMessage message, Session session)
        {
            throw new MessageHandlerException("Test Exception");
        }
    }
}
