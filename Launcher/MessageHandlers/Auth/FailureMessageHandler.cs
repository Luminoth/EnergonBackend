using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Auth;

namespace EnergonSoftware.Launcher.MessageHandlers.Auth
{
    internal sealed class FailureMessageHandler : IMessageHandler
    {
        internal FailureMessageHandler()
        {
        }

        public void HandleMessage(object context)
        {
            MessageHandlerContext ctx = (MessageHandlerContext)context;
            FailureMessage failure = (FailureMessage)ctx.Message;
            
            ClientState.Instance.AuthFailed(failure.Reason);
        }
    }
}
