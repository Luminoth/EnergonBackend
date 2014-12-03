using System.Threading.Tasks;
using System.Windows;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class LogoutMessageHandler : MessageHandler
    {
        private readonly OvermindSession _session;

        internal LogoutMessageHandler(OvermindSession session)
        {
            _session = session;
        }

        protected override void OnHandleMessage(IMessage message)
        {
            ((App)Application.Current).Logout();
        }
    }
}
