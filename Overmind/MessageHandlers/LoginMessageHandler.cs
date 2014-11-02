namespace EnergonSoftware.Overmind.MessageHandlers
{
    sealed class LoginMessageHandler : MessageHandler
    {
        internal LoginMessageHandler()
        {
        }

        protected override void OnHandleMessage(object context)
        {
            MessageHandlerContext ctx = (MessageHandlerContext)context;
        }
    }
}
