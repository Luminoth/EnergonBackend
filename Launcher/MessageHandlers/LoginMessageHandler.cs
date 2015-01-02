using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Launcher.Net;

namespace EnergonSoftware.Launcher.MessageHandlers
{
    internal sealed class LoginMessageHandler : MessageHandler
    {
        internal LoginMessageHandler()
        {
        }

        protected override void OnHandleMessage(IMessage message, Session session)
        {
            OvermindSession overmindSession = session as OvermindSession;
            if(null == overmindSession) {
                return;
            }

            ClientState.Instance.LoggingIn = false;
            ClientState.Instance.LoggedIn = true;

            ClientState.Instance.CurrentPage = ClientState.Page.Main;
        }
    }
}
