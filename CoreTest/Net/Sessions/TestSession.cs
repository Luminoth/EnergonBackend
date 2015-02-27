﻿using System.Net.Sockets;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Test.MessageHandlers;

namespace EnergonSoftware.Core.Test.Net.Sessions
{
    internal sealed class TestSessionFactory : ISessionFactory
    {
        public Session Create(Socket socket)
        {
            return new TestSession(socket);
        }
    }

    internal sealed class TestSession : Session
    {
        public override string Name { get { return "test"; } }

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override string FormatterType { get { return BinaryMessageFormatter.FormatterType; } }
        protected override IMessageHandlerFactory HandlerFactory { get { return new MessageHandlerFactory(); } }

        public TestSession() : base()
        {
        }

        public TestSession(Socket socket) : base(socket)
        {
        }

        public void QueueMessage(IMessage message)
        {
            Processor.QueueMessage(message);
        }
    }
}