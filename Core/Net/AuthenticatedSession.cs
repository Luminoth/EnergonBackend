using System.Net.Sockets;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.Messages;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public abstract class AuthenticatedSession : Session
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthenticatedSession));

        public Account Account { get; protected set; }

        public AuthenticatedSession(Socket socket) : base(socket)
        {
        }

        public bool Authenticate(Account account)
        {
            Logger.Debug("Authenticating account: " + account);
            return null != Account && null != account && Account.Equals(account);
        }

        public bool Authenticate(string username, string sessionid)
        {
            return Authenticate(new Account()
                {
                    Username = username,
                    SessionId = sessionid,
                    EndPoint = RemoteEndPoint,
                }
            );
        }

        protected abstract void LookupAccount(string username);

        public void Login(string username, string sessionid)
        {
            Logger.Info("Login request from username=" + username + ", with sessionid=" + sessionid);
            Account = null;

            LookupAccount(username);
            if(null == Account) {
                Error("Invalid login: " + username);
                return;
            }

            EnergonSoftware.Core.Accounts.Account account = new Account() { Username = username, SessionId = sessionid, EndPoint = RemoteEndPoint };
            if(!Authenticate(account)) {
                Error("Invalid login account: " + account + ", expected: " + Account);
                return;
            }
        }

        public void Logout()
        {
            SendMessage(new LogoutMessage());

            Disconnect();

            Account = null;
        }
    }
}
