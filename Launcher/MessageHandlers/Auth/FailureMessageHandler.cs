using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    internal sealed class FailureMessageHandler : MessageHandler
    {
        internal FailureMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, Session session)
        {
            FailureMessage failureMessage = (FailureMessage)message;
            AuthSession authSession = (AuthSession)session;

            await authSession.AuthFailedAsync(failureMessage.Reason).ConfigureAwait(false);
        }
    }
}
