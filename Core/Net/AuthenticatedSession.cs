using System.Net.Sockets;

using EnergonSoftware.Core.Accounts;

namespace EnergonSoftware.Core.Net
{
    public abstract class AuthenticatedSession : Session
    {
        public Account Account { get; set; }

        public AuthenticatedSession(SessionManager manager) : base(manager)
        {
        }

        public AuthenticatedSession(Socket socket, SessionManager manager) : base(socket, manager)
        {
        }
    }
}
