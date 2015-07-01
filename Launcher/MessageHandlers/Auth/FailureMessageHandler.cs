using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;

using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    internal sealed class FailureMessageHandler : MessageHandler
    {
        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            FailureMessage failureMessage = (FailureMessage)message;
            AuthSession authSession = (AuthSession)session;

            await authSession.AuthFailedAsync(failureMessage.Reason).ConfigureAwait(false);
        }
    }
}
