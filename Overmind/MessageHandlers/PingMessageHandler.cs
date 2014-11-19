using System.Threading.Tasks;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    sealed class PingMessageHandler : MessageHandler
    {
        private readonly LoginSession _session;

        internal PingMessageHandler(LoginSession session)
        {
            _session = session;
        }

        protected override Task OnHandleMessage(IMessage message)
        {
            return new Task(() =>
                {
                    _session.Ping();
                }
            );
        }
    }
}
