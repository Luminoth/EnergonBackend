using EnergonSoftware.Chat.Net;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Chat.MessageHandlers
{
    internal sealed class LogoutMessageHandler : MessageHandler
    {
        internal LogoutMessageHandler()
        {
        }

        protected override void OnHandleMessage(IMessage message, Session session)
        {
            ChatSession chatSession = (ChatSession)session;
            chatSession.Logout();
        }
    }
}
