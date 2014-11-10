using System.Net.Sockets;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Core.Test.Net
{
    sealed class TestSessionFactory : ISessionFactory
    {
        public Session CreateSession()
        {
            return new TestSession();
        }

        public Session CreateSession(Socket socket)
        {
            return new TestSession(socket);
        }
    }

    sealed class TestSession : Session
    {
        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public TestSession() : base()
        {
        }

        public TestSession(Socket socket) : base(socket)
        {
        }

        protected override void OnRun()
        {
        }
    }
}
