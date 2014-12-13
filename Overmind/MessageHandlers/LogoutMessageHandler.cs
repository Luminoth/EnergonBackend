using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class LogoutMessageHandler : MessageHandler
    {
        internal LogoutMessageHandler()
        {
        }

        protected async override void OnHandleMessage(IMessage message, Session session)
        {
            LoginSession loginSession = (LoginSession)session;
            await loginSession.Logout();
        }
    }
}
