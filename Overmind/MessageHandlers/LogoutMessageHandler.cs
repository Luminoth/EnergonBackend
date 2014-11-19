using System.Threading.Tasks;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    sealed class LogoutMessageHandler : MessageHandler
    {
        private readonly LoginSession _session;

        internal LogoutMessageHandler(LoginSession session)
        {
            _session = session;
        }

        protected override Task OnHandleMessage(IMessage message)
        {
            return new Task(() =>
                {
                    _session.Logout();
                }
            );
        }
    }
}
