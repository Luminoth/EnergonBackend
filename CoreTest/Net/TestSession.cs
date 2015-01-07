using System.Net.Sockets;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Test.MessageHandlers;

namespace EnergonSoftware.Core.Test.Net
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
        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        protected override IMessageHandlerFactory HandlerFactory { get { return new MessageHandlerFactory(); } }

        public TestSession() : base()
        {
        }

        public TestSession(Socket socket) : base(socket)
        {
        }
    }
}
