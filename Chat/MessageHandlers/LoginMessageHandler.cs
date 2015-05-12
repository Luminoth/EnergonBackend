using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;

using EnergonSoftware.Chat.Net;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Chat.MessageHandlers
{
    internal sealed class LoginMessageHandler : MessageHandler
    {
        internal LoginMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            LoginMessage loginMessage = (LoginMessage)message;
            ChatSession chatSession = (ChatSession)session;

            if(!await chatSession.LoginAsync(loginMessage.AccountName, loginMessage.SessionId).ConfigureAwait(false)) {
                return;
            }

            await chatSession.SyncFriends().ConfigureAwait(false);
        }
    }
}
