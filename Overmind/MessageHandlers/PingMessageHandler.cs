using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class PingMessageHandler : MessageHandler
    {
        private readonly LoginSession _session;

        internal PingMessageHandler(LoginSession session)
        {
            _session = session;
        }

        protected override void OnHandleMessage(IMessage message)
        {
            _session.Ping();
        }
    }
}
