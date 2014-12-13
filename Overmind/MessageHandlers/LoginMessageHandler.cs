using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Models;
using EnergonSoftware.Overmind.Net;

using log4net;

namespace EnergonSoftware.Overmind.MessageHandlers
{
    internal sealed class LoginMessageHandler : MessageHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LoginMessageHandler));

        internal LoginMessageHandler()
        {
        }

        protected async override void OnHandleMessage(IMessage message, Session session)
        {
            LoginMessage login = (LoginMessage)message;
            LoginSession loginSession = (LoginSession)session;
            await EventLogger.Instance.LoginRequestEvent(loginSession.RemoteEndPoint, login.Username);

            Logger.Info("New login attempt from user=" + login.Username + " and endpoint=" + loginSession.RemoteEndPoint);

            AccountInfo account = new AccountInfo() { Username = login.Username };
            using(DatabaseConnection connection = await DatabaseManager.AcquireDatabaseConnection()) {
                if(!await account.Read(connection)) {
                    await loginSession.LoginFailure(login.Username, "Bad Username");
                    return;
                }
            }

            if(!account.Active) {
                await loginSession.LoginFailure(account.Username, "Account Inactive");
                return;
            }

            if(!account.SessionId.Equals(login.Ticket, System.StringComparison.InvariantCultureIgnoreCase)) {
                await loginSession.LoginFailure(account.Username, "Invalid SessionId");
                return;
            }

            if(!NetUtil.CompareEndPoints(account.EndPoint, loginSession.RemoteEndPoint)) {
                Logger.Error("*** Possible spoof attempt from " + loginSession.RemoteEndPoint + " for account=" + account + "! ***");
                await loginSession.LoginFailure(account.Username, "Endpoint Mistmatch");
                return;
            }

            await loginSession.LoginSuccess(account);
        }
    }
}
