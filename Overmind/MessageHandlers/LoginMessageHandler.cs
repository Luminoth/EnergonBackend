using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;

using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class LoginMessageHandler : MessageHandler
    {
        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            LoginMessage loginMessage = (LoginMessage)message;
            OvermindSession overmindSession = (OvermindSession)session;

            if(!await overmindSession.LoginAsync(loginMessage.AccountName, loginMessage.SessionId).ConfigureAwait(false)) {
                return;
            }
        }
    }
}
