using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class PingMessageHandler : MessageHandler
    {
        internal PingMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            OvermindSession overmindSession = (OvermindSession)session;
            await overmindSession.PingAsync().ConfigureAwait(false);
        }
    }
}
