using System.Net.Sockets;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Launcher.Net
{
    sealed class OvermindSessionFactory : ISessionFactory
    {
        public Session CreateSession()
        {
            return new OvermindSession();
        }

        public Session CreateSession(Socket socket)
        {
            return new OvermindSession(socket);
        }
    }

    sealed class OvermindSession : Session
    {
        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public OvermindSession() : base()
        {
        }

        public OvermindSession(Socket socket) : base(socket)
        {
        }
    }
}
