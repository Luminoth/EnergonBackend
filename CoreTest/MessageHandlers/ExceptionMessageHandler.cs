using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Core.Test.MessageHandlers
{
    internal sealed class ExceptionMessageHandler : MessageHandler
    {
        internal ExceptionMessageHandler()
        {
        }

        protected override Task OnHandleMessageAsync(IMessage message, Session session)
        {
            throw new MessageHandlerException("Test Exception");
        }
    }
}
