using EnergonSoftware.Overmind.Net;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class LoginMessageHandler : MessageHandler
    {
        internal LoginMessageHandler()
        {
        }

        protected override void OnHandleMessage(IMessage message, Session session)
        {
            LoginMessage loginMessage = (LoginMessage)message;
            OvermindSession overmindSession = (OvermindSession)session;
            overmindSession.Login(loginMessage.Username, loginMessage.SessionId);
        }
    }
}
