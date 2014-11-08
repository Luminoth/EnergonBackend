using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    sealed class FailureMessageHandler : MessageHandler
    {
        AuthSession _session;

        internal FailureMessageHandler(AuthSession session)
        {
            _session = session;
        }

        protected override void OnHandleMessage(IMessage message)
        {
            FailureMessage failure = (FailureMessage)message;
            AuthManager.Instance.AuthFailed(failure.Reason);
        }
    }
}
