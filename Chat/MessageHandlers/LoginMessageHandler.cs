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

        protected override void OnHandleMessage(IMessage message, Session session)
        {
            LoginMessage loginMessage = (LoginMessage)message;
            ChatSession chatSession = (ChatSession)session;
            Task.Run(() => chatSession.Login(loginMessage.Username, loginMessage.SessionId)).Wait();
        }
    }
}
