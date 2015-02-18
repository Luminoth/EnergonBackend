using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class LoginMessageHandler : MessageHandler
    {
        internal LoginMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, Session session)
        {
            LoginMessage loginMessage = (LoginMessage)message;
            OvermindSession overmindSession = (OvermindSession)session;

            if(!await overmindSession.LoginAsync(loginMessage.AccountName, loginMessage.SessionId).ConfigureAwait(false)) {
                return;
            }
        }
    }
}
