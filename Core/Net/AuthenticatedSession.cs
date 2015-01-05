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

        protected abstract Task<Account> LookupAccountAsync(string username);

        public async Task LoginAsync(string username, string sessionid)
        {
            Logger.Info("Login request from username=" + username + ", with sessionid=" + sessionid);

            Account lookupAccount = await LookupAccountAsync(username).ConfigureAwait(false);
            if(null == lookupAccount) {
                Error("Invalid login: " + username);
                return;
            }
            Account = lookupAccount;

            EnergonSoftware.Core.Accounts.Account loginAccount = new Account()
            {
                Username = username,
                SessionId = sessionid,
                EndPoint = RemoteEndPoint
            };

            Logger.Debug("Authenticating login account: " + loginAccount);
            if(!Authenticate(loginAccount)) {
                Error("Invalid login account: " + loginAccount + ", expected: " + Account);
                return;
            }

            Logger.Info("Login for username=" + username + " successful!");
            await SendMessageAsync(new LoginMessage()).ConfigureAwait(false);
        }

        public async Task LogoutAsync()
        {
            await SendMessageAsync(new LogoutMessage()).ConfigureAwait(false);

            Disconnect();

            Account = null;
        }
    }
}
