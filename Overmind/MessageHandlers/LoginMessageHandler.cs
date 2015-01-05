using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net;
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
            await overmindSession.LoginAsync(loginMessage.Username, loginMessage.SessionId).ConfigureAwait(false);
        }
    }
}
