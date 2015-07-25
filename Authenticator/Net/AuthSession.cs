using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Authenticator.MessageHandlers;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Auth;
using EnergonSoftware.Backend.Net.Sessions;
using EnergonSoftware.Backend.Packet;

using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Packet;
using EnergonSoftware.Core.Serialization.Formatters;
using EnergonSoftware.Core.Util;

using EnergonSoftware.DAL;
using EnergonSoftware.DAL.Models.Accounts;

using log4net;

namespace EnergonSoftware.Authenticator.Net
{
    internal sealed class AuthSession : MessageNetworkSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthSession));

        public AuthType AuthType { get; private set; } = AuthType.None;

        public Nonce AuthNonce { get; private set; }

        public bool Authenticating { get; private set; }

        public bool Authenticated { get; private set; }

        public AccountInfo AccountInfo { get; private set; }

        public override string Name => "auth";

        // TODO: make this configurable
        public override int MaxSessionReadBufferSize => 1024 * 1000 * 10;

        protected override string MessageFormatterType => BinaryNetworkFormatter.FormatterType;

        protected override string PacketType => NetworkPacket.PacketType;

        public IMessageHandlerFactory MessageHandlerFactory => new AuthMessageHandlerFactory();

        public AuthSession(Socket socket) 
            : base(socket)
        {
            DataReceivedEvent += DataReceivedEventHandler;
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

// TODO: this should go into a processor class or something
// move the session buffer into the NetworkSession class
// the processor can cast the sender and work on the buffer?

        private async void DataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            SessionReadBuffer.Flip();

            IPacket packet = await new PacketReader().ReadAsync(new BackendPacketFactory(), SessionReadBuffer).ConfigureAwait(false);
            if(null == packet) {
                SessionReadBuffer.Reset();
                return;
            }

            if(Message.IsMessageContentType(packet.ContentType)) {
                Message message = await Message.DeSerializeAsync(packet.ContentType, packet.Encoding, packet.Content, 0, packet.ContentLength, new BackendMessageFactory()).ConfigureAwait(false);
                if(null == message) {
                    SessionReadBuffer.Reset();
                    return;
                }

// TODO: handle this shit
            }

            await SessionReadBuffer.CompactAsync().ConfigureAwait(false);
        }
    }
}
