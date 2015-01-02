using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    internal sealed class FailureMessageHandler : MessageHandler
    {
        internal FailureMessageHandler()
        {
        }

        protected override void OnHandleMessage(IMessage message, Session session)
        {
            FailureMessage failureMessage = (FailureMessage)message;
            AuthSession authSession = (AuthSession)session;

            authSession.AuthFailed(failureMessage.Reason);
        }
    }
}
