using System.Threading.Tasks;

using EnergonSoftware.Backend.Net.Sessions;
using EnergonSoftware.Backend.Properties;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Backend.MessageHandlers
{
    public class AuthenticatedMessageHandler : MessageHandler
    {
        private static void Authenticate(IAuthenticatedMessage message, AuthenticatedSession session)
        {
            if(null != message && null != session) {
                if(!session.Authenticate(message.AccountName, message.SessionId)) {
                    throw new MessageHandlerException(Resources.ErrorSessionNotAuthenticated);
                }
            }
        }

        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            await base.OnHandleMessageAsync(message, session).ConfigureAwait(false);

            Authenticate(message as IAuthenticatedMessage, session as AuthenticatedSession);
        }
    }
}
