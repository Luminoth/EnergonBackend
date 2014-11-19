using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    sealed class LogoutMessageHandler : MessageHandler
    {
        private readonly OvermindSession _session;

        internal LogoutMessageHandler(OvermindSession session)
        {
            _session = session;
        }

        protected override Task OnHandleMessage(IMessage message)
        {
            return new Task(() =>
                {
                    // TODO: should this log *everything* out?
                    _session.Logout();
                }
            );
        }
    }
}
