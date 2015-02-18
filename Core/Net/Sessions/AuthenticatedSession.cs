using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Properties;

using log4net;

namespace EnergonSoftware.Core.Net.Sessions
{
    public abstract class AuthenticatedSession : Session
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthenticatedSession));

        public Account Account { get; protected set; }

        protected AuthenticatedSession(Socket socket) : base(socket)
        {
        }

        public bool Authenticate(Account account)
        {
            return null != Account && null != account && Account.Equals(account);
        }

        public bool Authenticate(string account_name, string sessionid)
        {
            return Authenticate(new Account()
            {
                AccountName = account_name,
                SessionId = sessionid,
                EndPoint = RemoteEndPoint,
            });
        }

        public async Task<bool> LoginAsync(string account_name, string sessionid)
        {
            if(null != Account) {
                await ErrorAsync(Resources.ErrorSessionAlreadyAuthenticated).ConfigureAwait(false);
                return false;
            }

            Logger.Info("Login request from account_name=" + account_name + ", with sessionid=" + sessionid);

            Account lookupAccount = await LookupAccountAsync(account_name).ConfigureAwait(false);
            if(null == lookupAccount) {
                await ErrorAsync(string.Format(Resources.ErrorInvalidLogin, account_name)).ConfigureAwait(false);
                return false;
            }

            Account = lookupAccount;

            EnergonSoftware.Core.Accounts.Account loginAccount = new Account()
            {
                AccountName = account_name,
                SessionId = sessionid,
                EndPoint = RemoteEndPoint
            };

            Logger.Debug("Authenticating login account: " + loginAccount);
            if(!Authenticate(loginAccount)) {
                await ErrorAsync(string.Format(Resources.ErrorInvalidLoginAccount, loginAccount, Account)).ConfigureAwait(false);
                return false;
            }

            Logger.Info("Login for account_name=" + account_name + " successful!");
            await SendMessageAsync(new LoginMessage()).ConfigureAwait(false);

            return true;
        }

        public async Task LogoutAsync()
        {
            await SendMessageAsync(new LogoutMessage()).ConfigureAwait(false);
            await DisconnectAsync().ConfigureAwait(false);

            Account = null;
        }

        protected abstract Task<Account> LookupAccountAsync(string account_name);
    }
}
