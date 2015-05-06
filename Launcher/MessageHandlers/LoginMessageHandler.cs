using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Launcher.Net;
using EnergonSoftware.Launcher.Windows;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class LoginMessageHandler : MessageHandler
    {
        internal LoginMessageHandler()
        {
        }

        protected async override Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            OvermindSession overmindSession = session as OvermindSession;
            if(null == overmindSession) {
                return;
            }

            await MainWindow.ShowMainPageAsync().ConfigureAwait(false);
        }
    }
}
