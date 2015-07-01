using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Parser;
using EnergonSoftware.Backend.Net.Sessions;

using EnergonSoftware.Backend.Test.MessageHandlers;

namespace EnergonSoftware.Backend.Test.Net.Sessions
{
    internal sealed class TestSession : MessageSession
    {
        public override string Name { get { return "test"; } }

        public bool MessageProcessed { get; private set; }

        private readonly NetworkPacketParser _messageParser = new NetworkPacketParser();
        private readonly MessageProcessor _messageProcessor = new MessageProcessor();
        private readonly IMessageHandlerFactory _messageHandlerFactory = new MessageHandlerFactory();

        protected override string FormatterType { get { return BinaryMessageFormatter.FormatterType; } }

        public TestSession() : base()
        {
            DataReceivedEvent += _messageParser.DataReceivedEventHandlerAsync;
            _messageParser.MessageParsedEvent += _messageProcessor.MessageParsedEventHandler;
            _messageProcessor.HandleMessageEvent += HandleMessageEventHandler;
        }

        public void ProcessData(byte[] data)
        {
            MessageProcessed = false;

            OnDataReceived(data);
        }

        protected override MessagePacket CreatePacket(IMessage message)
        {
            return new NetworkPacket();
        }

        private async void HandleMessageEventHandler(object sender, HandleMessageEventArgs e)
        {
            MessageHandler handler = _messageHandlerFactory.Create(e.Message.Type);
            await handler.HandleMessageAsync(e.Message, this).ConfigureAwait(false);

            MessageProcessed = true;
        }
    }
}
