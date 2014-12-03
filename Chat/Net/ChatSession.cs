using System.Net.Sockets;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class ChatSessionFactory : ISessionFactory
    {
        public Session CreateSession(SessionManager manager)
        {
            return new ChatSession(manager);
        }

        public Session CreateSession(Socket socket, SessionManager manager)
        {
            return new ChatSession(socket, manager);
        }
    }

    internal sealed class ChatSession : Session
    {
        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public ChatSession(SessionManager manager) : base(manager)
        {
        }

        public ChatSession(Socket socket, SessionManager manager) : base(socket, manager)
        {
        }
    }
}
