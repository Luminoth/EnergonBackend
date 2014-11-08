using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    sealed class LogoutMessageHandler : MessageHandler
    {
        private OvermindSession _session;

        internal LogoutMessageHandler(OvermindSession session)
        {
            _session = session;
        }

        protected override void OnHandleMessage(IMessage message)
        {
            _session.Logout();
        }
    }
}
