using log4net;

using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    sealed class LoginMessageHandler : MessageHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LoginMessageHandler));

        internal LoginMessageHandler()
        {
        }

        protected override void OnHandleMessage(object context)
        {
            MessageHandlerContext ctx = (MessageHandlerContext)context;
            LoginMessage message = (LoginMessage)ctx.Message;

            EventLogger.Instance.LoginRequestEvent(ctx.Session.Socket.RemoteEndPoint, message.Username);

            _logger.Info("New login attempt from user=" + message.Username + " and endpoint=" + ctx.Session.Socket.RemoteEndPoint);

            AccountInfo account = new AccountInfo(message.Username);
            using(DatabaseConnection connection = ServerState.Instance.AcquireDatabaseConnection()) {
                if(!account.Read(connection)) {
                    ctx.Session.LoginFailure(message.Username, "Bad Username");
                    return;
                }
            }

            if(!account.Active) {
                ctx.Session.LoginFailure(account.Username, "Account Inactive");
                return;
            }

            if(!account.SessionId.Equals(message.Ticket, System.StringComparison.InvariantCultureIgnoreCase)) {
                ctx.Session.LoginFailure(account.Username, "Invalid SessionId");
                return;
            }

            if(!NetUtil.CompareEndPoints(account.SessionEndPoint, ctx.Session.Socket.RemoteEndPoint)) {
                _logger.Error("*** Possible spoof attempt from " + ctx.Session.Socket.RemoteEndPoint + " for account=" + account + "! ***");
                ctx.Session.LoginFailure(account.Username, "Endpoint Mistmatch");
                return;
            }

            ctx.Session.LoginSuccess(account);
        }
    }
}
