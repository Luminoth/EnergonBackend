using System.Net.Sockets;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Launcher.Net
{
    sealed class AuthSessionFactory : ISessionFactory
    {
        public Session CreateSession()
        {
            return new AuthSession();
        }

        public Session CreateSession(Socket socket)
        {
            return new AuthSession(socket);
        }
    }

    sealed class AuthSession : Session
    {
        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public AuthSession() : base()
        {
        }

        public AuthSession(Socket socket) : base(socket)
        {
        }
    }
}
