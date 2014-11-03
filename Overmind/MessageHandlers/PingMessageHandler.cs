namespace EnergonSoftware.Overmind.MessageHandlers
{
    sealed class PingMessageHandler : MessageHandler
    {
        internal PingMessageHandler()
        {
        }

        protected override void OnHandleMessage(object context)
        {
            MessageHandlerContext ctx = (MessageHandlerContext)context;
            ctx.Session.Ping();
        }
    }
}
