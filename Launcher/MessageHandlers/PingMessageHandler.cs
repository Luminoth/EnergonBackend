using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class PingMessageHandler : MessageHandler
    {
        internal PingMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, Session session)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}
