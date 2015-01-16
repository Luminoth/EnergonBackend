using System.Threading.Tasks;

using EnergonSoftware.Chat.Net;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Chat.MessageHandlers
{
    internal sealed class PingMessageHandler : MessageHandler
    {
        internal PingMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, Session session)
        {
            ChatSession chatSession = (ChatSession)session;
            await chatSession.PingAsync().ConfigureAwait(false);
        }
    }
}
