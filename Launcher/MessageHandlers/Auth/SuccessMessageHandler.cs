using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    internal sealed class SuccessMessageHandler : MessageHandler
    {
        internal SuccessMessageHandler()
        {
        }
        
        protected override void OnHandleMessage(object context)
        {
            MessageHandlerContext ctx = (MessageHandlerContext)context;
            SuccessMessage message = (SuccessMessage)ctx.Message;

            ClientState.Instance.AuthSuccess(message.SessionId);
        }
    }
}
