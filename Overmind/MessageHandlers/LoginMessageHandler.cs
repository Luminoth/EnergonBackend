using System.Threading.Tasks;

using log4net;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    sealed class LoginMessageHandler : MessageHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LoginMessageHandler));

        private readonly LoginSession _session;

        internal LoginMessageHandler(LoginSession session)
        {
            _session = session;
        }

        protected override Task OnHandleMessage(IMessage message)
        {
            return new Task(() =>
                {
                    LoginMessage login = (LoginMessage)message;
                    EventLogger.Instance.LoginRequestEvent(_session.RemoteEndPoint, login.Username);

                    _logger.Info("New login attempt from user=" + login.Username + " and endpoint=" + _session.RemoteEndPoint);

                    AccountInfo account = new AccountInfo(login.Username);
                    using(DatabaseConnection connection = DatabaseManager.AcquireDatabaseConnection()) {
                        if(!account.Read(connection)) {
                            _session.LoginFailure(login.Username, "Bad Username");
                            return;
                        }
                    }

                    if(!account.Active) {
                        _session.LoginFailure(account.Username, "Account Inactive");
                        return;
                    }

                    if(!account.SessionId.Equals(login.Ticket, System.StringComparison.InvariantCultureIgnoreCase)) {
                        _session.LoginFailure(account.Username, "Invalid SessionId");
                        return;
                    }

                    if(!NetUtil.CompareEndPoints(account.SessionEndPoint, _session.RemoteEndPoint)) {
                        _logger.Error("*** Possible spoof attempt from " + _session.RemoteEndPoint + " for account=" + account + "! ***");
                        _session.LoginFailure(account.Username, "Endpoint Mistmatch");
                        return;
                    }

                    _session.LoginSuccess(account);
                }
            );
        }
    }
}
