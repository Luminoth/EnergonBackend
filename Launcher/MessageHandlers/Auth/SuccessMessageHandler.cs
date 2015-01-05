using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    internal sealed class SuccessMessageHandler : MessageHandler
    {
        internal SuccessMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, Session session)
        {
            SuccessMessage successMessage = (SuccessMessage)message;
            AuthSession authSession = (AuthSession)session;

            await authSession.AuthSuccessAsync(successMessage.SessionId).ConfigureAwait(false);
        }
    }
}
