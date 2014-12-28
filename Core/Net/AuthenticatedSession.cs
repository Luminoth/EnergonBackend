using System.Net.Sockets;

using EnergonSoftware.Core.Accounts;

namespace EnergonSoftware.Core.Net
{
    public abstract class AuthenticatedSession : Session
    {
        public Account Account { get; set; }

        public AuthenticatedSession(Socket socket) : base(socket)
        {
        }
    }
}
