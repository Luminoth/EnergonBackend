using System.Net.Sockets;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Authenticator.Net
{
    sealed class InstanceNotifierSessionFactory : ISessionFactory
    {
        public Session CreateSession(SessionManager manager)
        {
            return new InstanceNotifierSession(manager);
        }

        public Session CreateSession(Socket socket, SessionManager manager)
        {
            return new InstanceNotifierSession(socket, manager);
        }
    }

    sealed class InstanceNotifierSession : Session
    {
        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public InstanceNotifierSession(SessionManager manager) : base(manager)
        {
        }

        public InstanceNotifierSession(Socket socket, SessionManager manager) : base(socket, manager)
        {
        }

        protected override void OnRun()
        {
        }
    }
}
