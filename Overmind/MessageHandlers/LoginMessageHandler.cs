namespace EnergonSoftware.Overmind.MessageHandlers
{
    sealed class LoginMessageHandler : IMessageHandler
    {
        internal LoginMessageHandler()
        {
        }

        public void HandleMessage(object context)
        {
            MessageHandlerContext ctx = (MessageHandlerContext)context;
        }
    }
}
