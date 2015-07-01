using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class LogoutMessageHandler : MessageHandler
    {
        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            await App.Instance.LogoutAsync().ConfigureAwait(false);
        }
    }
}
