using System;
using System.Configuration;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Authenticator.MessageHandlers;
using EnergonSoftware.Core;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Models.Accounts;

using log4net;

namespace EnergonSoftware.Authenticator.Net
{
    internal sealed class AuthSessionFactory : ISessionFactory
    {
        public Session Create(Socket socket)
        {
            AuthSession session = null;
            try {
                session = new AuthSession(socket);
                session.Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
                return session;
            } catch(Exception) {
                if(null != session) {
                    session.Dispose();
                }

                throw;
            }
        }
    }

    internal sealed class AuthSession : Session
    {
        public AuthType AuthType { get; private set; }
        public Nonce AuthNonce { get; private set; }

        public bool Authenticating { get; private set; }
        public bool Authenticated { get; private set; }

        public AccountInfo AccountInfo { get; private set; }

        public override string Name { get { return "auth"; } }

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override string FormatterType { get { return BinaryMessageFormatter.FormatterType; } }
        protected override IMessageHandlerFactory HandlerFactory { get { return new AuthMessageHandlerFactory(); } }

        public AuthSession(Socket socket) : base(socket)
        {
            AuthType = AuthType.None;
        }

        public async Task ChallengeAsync(AuthType type, Nonce nonce, string challenge)
        {
            AuthType = type;
            AuthNonce = nonce;

            Authenticating = true;
            Authenticated = false;

            await SendMessageAsync(new ChallengeMessage()
                {
                    Challenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(challenge)),
                }).ConfigureAwait(false);
        }

        public async Task ChallengeAsync(string challenge, AccountInfo accountInfo)
        {
            await InstanceNotifier.Instance.AuthenticatingAsync(accountInfo.AccountName, RemoteEndPoint).ConfigureAwait(false);

            AccountInfo = accountInfo;
            Authenticated = true;

            await SendMessageAsync(new ChallengeMessage()
                {
                    Challenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(challenge)),
                }).ConfigureAwait(false);
        }

        public async Task SuccessAsync(string sessionid)
        {
            await InstanceNotifier.Instance.AuthenticatedAsync(AccountInfo.AccountName, sessionid, RemoteEndPoint).ConfigureAwait(false);
            await EventLogger.Instance.SuccessEventAsync(RemoteEndPoint, AccountInfo.AccountName).ConfigureAwait(false);

            AccountInfo.SessionId = sessionid;
            AccountInfo.EndPoint = RemoteEndPoint.ToString();

            using(DatabaseConnection connection = await DatabaseManager.AcquireDatabaseConnectionAsync().ConfigureAwait(false)) {
                await AccountInfo.UpdateAsync(connection).ConfigureAwait(false);
            }

            await SendMessageAsync(new SuccessMessage()
                {
                    SessionId = sessionid,
                }).ConfigureAwait(false);

            await DisconnectAsync().ConfigureAwait(false);
        }

        public async Task FailureAsync(string reason)
        {
            await EventLogger.Instance.FailedEventAsync(RemoteEndPoint, null == AccountInfo ? null : AccountInfo.AccountName, reason).ConfigureAwait(false);

            /*
             * NOTE: we don't update the database here because this
             * might be a malicious login attempt against an account
             * that is already using a legitimate ticket
             */

            AccountInfo = null;

            await SendMessageAsync(new FailureMessage()
                {
                    Reason = reason,
                }).ConfigureAwait(false);

            await DisconnectAsync().ConfigureAwait(false);
        }
    }
}
