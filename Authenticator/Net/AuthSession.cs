using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Authenticator.MessageHandlers;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages.Auth;
using EnergonSoftware.Backend.Net.Sessions;
using EnergonSoftware.Backend.Packet;

using EnergonSoftware.Core.Serialization.Formatters;
using EnergonSoftware.Core.Util;

using EnergonSoftware.DAL;
using EnergonSoftware.DAL.Models.Accounts;

namespace EnergonSoftware.Authenticator.Net
{
    internal sealed class AuthSession : MessageNetworkSession
    {
        public AuthType AuthType { get; private set; } = AuthType.None;

        public Nonce AuthNonce { get; private set; }

        public bool Authenticating { get; private set; }

        public bool Authenticated { get; private set; }

        public AccountInfo AccountInfo { get; private set; }

        public override string Name => "auth";

        // TODO: make this configurable
        public override int MaxSessionReceiveBufferSize => 1024 * 1000 * 10;

        protected override string MessageFormatterType => BinaryNetworkFormatter.FormatterType;

        protected override string PacketType => NetworkPacket.PacketType;

        public override IMessageHandlerFactory MessageHandlerFactory => new AuthMessageHandlerFactory();

        public AuthSession(Socket socket) 
            : base(socket)
        {
            MessageReceivedEvent += Authenticator.Instance.MessageProcessor.MessageReceivedEventHandler;
        }

        public async Task ChallengeAsync(AuthType type, Nonce nonce, string challenge)
        {
            AuthType = type;
            AuthNonce = nonce;

            Authenticating = true;
            Authenticated = false;

            await SendAsync(new ChallengeMessage()
                {
                    Challenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(challenge)),
                }).ConfigureAwait(false);
        }

        public async Task ChallengeAsync(string challenge, AccountInfo accountInfo)
        {
            await InstanceNotifier.Instance.AuthenticatingAsync(accountInfo.AccountName, RemoteEndPoint).ConfigureAwait(false);

            AccountInfo = accountInfo;
            Authenticated = true;

            await SendAsync(new ChallengeMessage()
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

            await SendAsync(new SuccessMessage()
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

            await SendAsync(new FailureMessage()
                {
                    Reason = reason,
                }).ConfigureAwait(false);

            await DisconnectAsync().ConfigureAwait(false);
        }
    }
}
