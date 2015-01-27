﻿using System.Net.Sockets;
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

        public async Task<bool> LoginAsync(string username, string sessionid)
        {
            if(null != Account) {
                await ErrorAsync(Resources.ErrorSessionAlreadyAuthenticated).ConfigureAwait(false);
                return false;
            }

            Logger.Info("Login request from username=" + username + ", with sessionid=" + sessionid);

            Account lookupAccount = await LookupAccountAsync(username).ConfigureAwait(false);
            if(null == lookupAccount) {
                await ErrorAsync(string.Format(Resources.ErrorInvalidLogin, username)).ConfigureAwait(false);
                return false;
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
                await ErrorAsync(string.Format(Resources.ErrorInvalidLoginAccount, loginAccount, Account)).ConfigureAwait(false);
                return false;
            }

            Logger.Info("Login for username=" + username + " successful!");
            await SendMessageAsync(new LoginMessage()).ConfigureAwait(false);

            return true;
        }

        public async Task LogoutAsync()
        {
            await SendMessageAsync(new LogoutMessage()).ConfigureAwait(false);
            await DisconnectAsync().ConfigureAwait(false);

            Account = null;
        }
    }
}