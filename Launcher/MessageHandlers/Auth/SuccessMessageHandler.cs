using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    sealed class SuccessMessageHandler : MessageHandler
    {
        private readonly AuthSession _session;

        internal SuccessMessageHandler(AuthSession session)
        {
            _session = session;
        }
        
        protected override void OnHandleMessage(IMessage message)
        {
            SuccessMessage success = (SuccessMessage)message;
            _session.AuthSuccess(success.SessionId);
        }
    }
}
