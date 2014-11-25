using System.Net.Sockets;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Core.Test.Net
{
    internal sealed class TestSessionFactory : ISessionFactory
    {
        public Session CreateSession(SessionManager manager)
        {
            return new TestSession(manager);
        }

        public Session CreateSession(Socket socket, SessionManager manager)
        {
            return new TestSession(socket, manager);
        }
    }

    internal sealed class TestSession : Session
    {
        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public TestSession(SessionManager manager) : base(manager)
        {
        }

        public TestSession(Socket socket, SessionManager manager) : base(socket, manager)
        {
        }
    }
}
