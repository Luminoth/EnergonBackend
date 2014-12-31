using System.Net.Sockets;

using EnergonSoftware.Core.Accounts;

namespace EnergonSoftware.Core.Net
{
    public abstract class AuthenticatedSession : Session
    {
        public Account Account { get; protected set; }

        public AuthenticatedSession(Socket socket) : base(socket)
        {
        }

        public bool Authenticate(string username, string sessionid)
        {
            return null != Account && Account.Equals(new Account()
                {
                    Username = username,
                    SessionId = sessionid,
                    EndPoint = RemoteEndPoint,
                }
            );
        }

        public abstract void Login(string username, string sessionid);
        public abstract void Logout();
    }
}
