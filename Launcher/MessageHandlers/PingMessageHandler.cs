using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class PingMessageHandler : MessageHandler
    {
        internal PingMessageHandler()
        {
        }

        protected override void OnHandleMessage(IMessage message)
        {
        }
    }
}
