using System.Threading.Tasks;

using EnergonSoftware.Chat.Net;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Chat.MessageHandlers
{
    internal sealed class LoginMessageHandler : MessageHandler
    {
        internal LoginMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, Session session)
        {
            LoginMessage loginMessage = (LoginMessage)message;
            ChatSession chatSession = (ChatSession)session;
            await chatSession.LoginAsync(loginMessage.Username, loginMessage.SessionId).ConfigureAwait(false);
        }
    }
}
