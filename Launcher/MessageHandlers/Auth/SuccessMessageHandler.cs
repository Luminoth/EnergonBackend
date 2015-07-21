using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;

using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    internal sealed class SuccessMessageHandler : MessageHandler
    {
        protected async override Task OnHandleMessageAsync(Message message, NetworkSession session)
        {
            SuccessMessage successMessage = (SuccessMessage)message;
            AuthSession authSession = (AuthSession)session;

            await authSession.AuthSuccessAsync(successMessage.SessionId).ConfigureAwait(false);
        }
    }
}
