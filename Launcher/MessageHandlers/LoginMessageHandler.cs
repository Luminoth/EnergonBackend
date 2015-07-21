using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;

using EnergonSoftware.Core.Net.Sessions;

using EnergonSoftware.Launcher.Net;
using EnergonSoftware.Launcher.Windows;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class LoginMessageHandler : MessageHandler
    {
        protected async override Task OnHandleMessageAsync(Message message, NetworkSession session)
        {
            OvermindSession overmindSession = session as OvermindSession;
            if(null == overmindSession) {
                return;
            }

            await MainWindow.ShowMainPageAsync().ConfigureAwait(false);
        }
    }
}
