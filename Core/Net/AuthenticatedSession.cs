using System.Net.Sockets;
using System.Threading.Tasks;

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

        public async Task Login(string username, string sessionid)
        {
            Logger.Info("Login request from username=" + username + ", with sessionid=" + sessionid);
            Account = null;

            await Task.Run(() => LookupAccount(username));
            if(null == Account) {
                Error("Invalid login: " + username);
                return;
            }

            EnergonSoftware.Core.Accounts.Account account = new Account() { Username = username, SessionId = sessionid, EndPoint = RemoteEndPoint };
            Logger.Debug("Authenticating login account: " + account);
            if(!Authenticate(account)) {
                Error("Invalid login account: " + account + ", expected: " + Account);
                return;
            }

            Logger.Info("Login for username=" + username + " successful!");
            SendMessage(new LoginMessage());
        }

        public void Logout()
        {
            SendMessage(new LogoutMessage());

            Disconnect();

            Account = null;
        }
    }
}
