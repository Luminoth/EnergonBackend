using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;
using EnergonSoftware.Backend.Messages.Auth;
using EnergonSoftware.Backend.Properties;

using log4net;

namespace EnergonSoftware.Backend.Net.Sessions
{
    /// <summary>
    /// Extends the NetworkSession to associate an Account with the session
    /// </summary>
    public abstract class AuthenticatedNetworkSession : MessageNetworkSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthenticatedNetworkSession));

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>
        /// The account.
        /// </value>
        public Account Account { get; protected set; }

        /// <summary>
        /// Authenticates the specified account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns>True if the account is authenticated</returns>
        public bool Authenticate(Account account)
        {
            return null != Account && null != account && Account.Equals(account);
        }

        /// <summary>
        /// Authenticates the specified account.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="sessionid">The sessionid.</param>
        /// <returns>True if the account is authenticated</returns>
        public bool Authenticate(string accountName, string sessionid)
        {
            return Authenticate(new Account
            {
                AccountName = accountName,
                SessionId = sessionid,
                EndPoint = RemoteEndPoint,
            });
        }

        /// <summary>
        /// Logs in the account
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="sessionid">The sessionid.</param>
        /// <returns>True if the login was successful</returns>
        public async Task<bool> LoginAsync(string accountName, string sessionid)
        {
            if(null != Account) {
                await ErrorAsync(Resources.ErrorSessionAlreadyAuthenticated).ConfigureAwait(false);
                return false;
            }

            Logger.Info($"Login request from accountName={accountName}, with sessionid={sessionid}");

            Account lookupAccount = await LookupAccountAsync(accountName).ConfigureAwait(false);
            if(null == lookupAccount) {
                await ErrorAsync(string.Format(Resources.ErrorInvalidLogin, accountName)).ConfigureAwait(false);
                return false;
            }

            Account = lookupAccount;

            Account loginAccount = new Account()
            {
                AccountName = accountName,
                SessionId = sessionid,
                EndPoint = RemoteEndPoint
            };

            Logger.Debug($"Authenticating login account: {loginAccount}");
            if(!Authenticate(loginAccount)) {
                await ErrorAsync(string.Format(Resources.ErrorInvalidLoginAccount, loginAccount, Account)).ConfigureAwait(false);
                return false;
            }

            Logger.Info($"Login for accountName={accountName} successful!");
            await SendAsync(new LoginMessage()).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Logs the account out and disconnects the session.
        /// </summary>
        public async Task LogoutAsync()
        {
            await SendAsync(new LogoutMessage()).ConfigureAwait(false);
            await DisconnectAsync().ConfigureAwait(false);

            Account = null;
        }

        /// <summary>
        /// Looks up the account.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <returns>The account</returns>
        protected abstract Task<Account> LookupAccountAsync(string accountName);

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatedNetworkSession"/> class.
        /// </summary>
        protected AuthenticatedNetworkSession()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatedNetworkSession"/> class.
        /// </summary>
        /// <param name="socket">The already connected socket to wrap.</param>
        protected AuthenticatedNetworkSession(Socket socket)
            : base(socket)
        {
        }
    }
}
