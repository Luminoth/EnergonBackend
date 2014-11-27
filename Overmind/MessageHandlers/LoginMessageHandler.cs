using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects;
using EnergonSoftware.Overmind.Net;

using log4net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class LoginMessageHandler : MessageHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginMessageHandler));

        private readonly LoginSession _session;

        internal LoginMessageHandler(LoginSession session)
        {
            _session = session;
        }

        protected async override void OnHandleMessage(IMessage message)
        {
            LoginMessage login = (LoginMessage)message;
            await EventLogger.Instance.LoginRequestEvent(_session.RemoteEndPoint, login.Username);

            Logger.Info("New login attempt from user=" + login.Username + " and endpoint=" + _session.RemoteEndPoint);

            AccountInfo account = new AccountInfo(login.Username);
            using(DatabaseConnection connection = await DatabaseManager.AcquireDatabaseConnection()) {
                if(!await account.Read(connection)) {
                    await _session.LoginFailure(login.Username, "Bad Username");
                    return;
                }
            }

            if(!account.Active) {
                await _session.LoginFailure(account.Username, "Account Inactive");
                return;
            }

            if(!account.SessionId.Equals(login.Ticket, System.StringComparison.InvariantCultureIgnoreCase)) {
                await _session.LoginFailure(account.Username, "Invalid SessionId");
                return;
            }

            if(!NetUtil.CompareEndPoints(account.SessionEndPoint, _session.RemoteEndPoint)) {
                Logger.Error("*** Possible spoof attempt from " + _session.RemoteEndPoint + " for account=" + account + "! ***");
                await _session.LoginFailure(account.Username, "Endpoint Mistmatch");
                return;
            }

            await _session.LoginSuccess(account);
        }
    }
}
