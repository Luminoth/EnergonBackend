using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    internal sealed class SuccessMessageHandler : IMessageHandler
    {
        internal SuccessMessageHandler()
        {
        }
        
        public void HandleMessage(object context)
        {
            MessageHandlerContext ctx = (MessageHandlerContext)context;
            SuccessMessage success = (SuccessMessage)ctx.Message;

            ClientState.Instance.AuthSuccess(success.SessionId);
        }
    }
}
