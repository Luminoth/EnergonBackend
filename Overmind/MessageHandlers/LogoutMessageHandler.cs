using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class LogoutMessageHandler : MessageHandler
    {
        internal LogoutMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, Session session)
        {
            OvermindSession overmindSession = (OvermindSession)session;
            await overmindSession.LogoutAsync().ConfigureAwait(false);
        }
    }
}
