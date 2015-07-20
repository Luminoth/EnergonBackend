﻿using System.Net.Sockets;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Parser;
using EnergonSoftware.Backend.Net.Sessions;

using EnergonSoftware.Manager.MessageHandlers;


namespace EnergonSoftware.Manager.Net
{
    internal sealed class InstanceNotifierSession : MessageSession
    {
        public override string Name => "instanceNotifier";

        private readonly NetworkPacketParser _messageParser = new NetworkPacketParser();
        private readonly MessageProcessor _messageProcessor = new MessageProcessor();
        private readonly IMessageHandlerFactory _messageHandlerFactory = new InstanceNotifierMessageHandlerFactory();

        protected override string FormatterType => BinaryMessageFormatter.FormatterType;

        public InstanceNotifierSession(Socket socket) : base(socket)
        {
            DataReceivedEvent += _messageParser.DataReceivedEventHandlerAsync;
            _messageParser.MessageParsedEvent += _messageProcessor.MessageParsedEventHandler;
            _messageProcessor.HandleMessageEvent += HandleMessageEventHandler;
        }

        protected override MessagePacket CreatePacket(IMessage message)
        {
            return new NetworkPacket();
        }

        private async void HandleMessageEventHandler(object sender, HandleMessageEventArgs e)
        {
            MessageHandler handler = _messageHandlerFactory.Create(e.Message.Type);
            await handler.HandleMessageAsync(e.Message, this).ConfigureAwait(false);
        }
    }
}
