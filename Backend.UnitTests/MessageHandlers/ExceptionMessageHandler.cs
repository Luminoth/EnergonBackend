using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Backend.UnitTests.MessageHandlers
{
    internal sealed class ExceptionMessageHandler : MessageHandler
    {
        protected override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            throw new MessageHandlerException("Test Exception");
        }
    }
}
