using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class PingMessageHandler : MessageHandler
    {
        protected async override Task OnHandleMessageAsync(Message message, NetworkSession session)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}
