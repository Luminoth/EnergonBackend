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
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Models;

using log4net;

namespace EnergonSoftware.Authenticator.Net
{
    internal sealed class AuthSessionFactory : ISessionFactory
    {
        public Session Create(Socket socket)
        {
            return new AuthSession(socket)
            {
                Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]),
            };
        }
    }

    internal sealed class AuthSession : Session
    {
        public AuthType AuthType { get; private set; }
        public Nonce AuthNonce { get; private set; }

        public bool Authenticating { get; private set; }
        public bool Authenticated { get; private set; }

        public AccountInfo AccountInfo { get; private set; }

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new AuthMessageHandlerFactory(); } }

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
                }
            ).ConfigureAwait(false);
        }

        public async Task ChallengeAsync(string challenge, AccountInfo accountInfo)
        {
            await InstanceNotifier.Instance.AuthenticatingAsync(accountInfo.Username, RemoteEndPoint).ConfigureAwait(false);

            AccountInfo = accountInfo;
            Authenticated = true;

            await SendMessageAsync(new ChallengeMessage()
                {
                    Challenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(challenge)),
                }
            ).ConfigureAwait(false);
        }

        public async Task SuccessAsync(string sessionid)
        {
            await InstanceNotifier.Instance.AuthenticatedAsync(AccountInfo.Username, sessionid, RemoteEndPoint).ConfigureAwait(false);
            await EventLogger.Instance.SuccessEventAsync(RemoteEndPoint, AccountInfo.Username).ConfigureAwait(false);

            AccountInfo.SessionId = sessionid;
            AccountInfo.EndPoint = RemoteEndPoint.ToString();

            using(DatabaseConnection connection = await DatabaseManager.AcquireDatabaseConnectionAsync().ConfigureAwait(false)) {
                await AccountInfo.UpdateAsync(connection).ConfigureAwait(false);
            }

            await SendMessageAsync(new SuccessMessage()
                {
                    SessionId = sessionid,
                }
            ).ConfigureAwait(false);

            Disconnect();
        }

        public async Task FailureAsync(string reason)
        {
            await EventLogger.Instance.FailedEventAsync(RemoteEndPoint, null == AccountInfo ? null : AccountInfo.Username, reason).ConfigureAwait(false);

            // NOTE: we don't update the database here because this
            // might be a malicious login attempt against an account
            // that is already using a legitimate ticket

            AccountInfo = null;

            await SendMessageAsync(new FailureMessage()
                {
                    Reason = reason,
                }
            ).ConfigureAwait(false);

            Disconnect();
        }
    }
}
