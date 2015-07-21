using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;

using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class PingMessageHandler : MessageHandler
    {
        protected async override Task OnHandleMessageAsync(Message message, NetworkSession session)
        {
            OvermindSession overmindSession = (OvermindSession)session;
            await overmindSession.PingAsync().ConfigureAwait(false);
        }
    }
}
