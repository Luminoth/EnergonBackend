using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;

using EnergonSoftware.Chat.Net;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Chat.MessageHandlers
{
    internal sealed class PingMessageHandler : MessageHandler
    {
        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            ChatSession chatSession = (ChatSession)session;
            await chatSession.PingAsync().ConfigureAwait(false);
        }
    }
}
