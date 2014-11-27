using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class LogoutMessageHandler : MessageHandler
    {
        private readonly LoginSession _session;

        internal LogoutMessageHandler(LoginSession session)
        {
            _session = session;
        }

        protected async override void OnHandleMessage(IMessage message)
        {
            await _session.Logout();
        }
    }
}
