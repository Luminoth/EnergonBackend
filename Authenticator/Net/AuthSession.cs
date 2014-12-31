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
        private readonly object _lock = new object();

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

        public void Challenge(AuthType type, Nonce nonce, string challenge)
        {
            lock(_lock) {
                AuthType = type;
                AuthNonce = nonce;

                Authenticating = true;
                Authenticated = false;
            }

            SendMessage(new ChallengeMessage()
                {
                    Challenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(challenge)),
                }
            );
        }

        public async Task Challenge(string challenge, AccountInfo accountInfo)
        {
            await InstanceNotifier.Instance.Authenticating(accountInfo.Username, RemoteEndPoint);

            lock(_lock) {
                AccountInfo = accountInfo;
                Authenticated = true;
            }

            SendMessage(new ChallengeMessage()
                {
                    Challenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(challenge)),
                }
            );
        }

        public async Task Success(string sessionid)
        {
            await InstanceNotifier.Instance.Authenticated(AccountInfo.Username, sessionid, RemoteEndPoint);
            await EventLogger.Instance.SuccessEvent(RemoteEndPoint, AccountInfo.Username);

            lock(_lock) {
                AccountInfo.SessionId = sessionid;
                AccountInfo.EndPoint = RemoteEndPoint.ToString();
            }

            using(DatabaseConnection connection = await DatabaseManager.AcquireDatabaseConnection()) {
                await AccountInfo.Update(connection);
            }

            SendMessage(new SuccessMessage()
                {
                    SessionId = sessionid,
                }
            );

            Disconnect();
        }

        public async Task Failure(string reason)
        {
            await EventLogger.Instance.FailedEvent(RemoteEndPoint, null == AccountInfo ? null : AccountInfo.Username, reason);

            // NOTE: we don't update the database here because this
            // might be a malicious login attempt against an account
            // that is already using a legitimate ticket

            lock(_lock) {
                AccountInfo = null;
            }

            SendMessage(new FailureMessage()
                {
                    Reason = reason,
                }
            );

            Disconnect();
        }
    }
}
