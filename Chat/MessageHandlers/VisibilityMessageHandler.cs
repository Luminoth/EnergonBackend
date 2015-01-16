using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Chat.MessageHandlers
{
    internal sealed class VisibilityMessageHandler : MessageHandler
    {
        internal VisibilityMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, Session session)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}
