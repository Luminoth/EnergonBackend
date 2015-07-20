using System;
using System.Configuration;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Authenticator.MessageHandlers;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;
using EnergonSoftware.Backend.Messages.Parser;
using EnergonSoftware.Backend.Net.Sessions;

using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Util;

using EnergonSoftware.DAL;
using EnergonSoftware.DAL.Models.Accounts;


namespace EnergonSoftware.Authenticator.Net
{
    // TODO: move this into its own file
    internal sealed class AuthSessionFactory : INetworkSessionFactory
    {
        public NetworkSession Create(Socket socket)
        {
            AuthSession session = null;
            try {
                session = new AuthSession(socket)
                {
                    TimeoutMs = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]),
                };
                return session;
            } catch(Exception) {
                session?.Dispose();
                throw;
            }
        }
    }

    internal sealed class AuthSession : MessageSession
    {
        public AuthType AuthType { get; private set; }

        public Nonce AuthNonce { get; private set; }

        public bool Authenticating { get; private set; }

        public bool Authenticated { get; private set; }

        public AccountInfo AccountInfo { get; private set; }

        public override string Name => "auth";

        private readonly NetworkPacketParser _messageParser = new NetworkPacketParser();
        private readonly MessageProcessor _messageProcessor = new MessageProcessor();
        private readonly IMessageHandlerFactory _messageHandlerFactory = new AuthMessageHandlerFactory();

        protected override string FormatterType => BinaryMessageFormatter.FormatterType;

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

            using(AccountsDatabaseContext context = new AccountsDatabaseContext()) {
                context.Accounts.Attach(AccountInfo);

                AccountInfo.SessionId = sessionid;
                AccountInfo.EndPoint = RemoteEndPoint.ToString();

                await context.SaveChangesAsync().ConfigureAwait(false);
            }

            await SendMessageAsync(new SuccessMessage()
                {
                    SessionId = sessionid,
                }).ConfigureAwait(false);

            await DisconnectAsync().ConfigureAwait(false);
        }

        public async Task FailureAsync(string reason)
        {
            await EventLogger.Instance.FailedEventAsync(RemoteEndPoint, AccountInfo?.AccountName, reason).ConfigureAwait(false);

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

        protected override MessagePacket CreatePacket(IMessage message)
        {
            return new NetworkPacket();
        }
    }
}
