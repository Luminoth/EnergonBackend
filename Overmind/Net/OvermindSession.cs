using System;
using System.Configuration;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Models;
using EnergonSoftware.Overmind.MessageHandlers;

using log4net;

namespace EnergonSoftware.Overmind.Net
{
    internal sealed class OvermindSessionFactory : ISessionFactory
    {
        public Session Create(Socket socket)
        {
            return new OvermindSession(socket)
            {
                Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]),
            };
        }
    }

    internal sealed class OvermindSession : AuthenticatedSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OvermindSession));

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new OvermindMessageHandlerFactory(); } }

        public OvermindSession(Socket socket) : base(socket)
        {
        }

        public override void Login(string username, string sessionid)
        {
            AccountInfo account = new AccountInfo() { Username = username };
            using(DatabaseConnection connection = Task.Run(() => DatabaseManager.AcquireDatabaseConnection()).Result) {
                if(!Task.Run(() => account.Read(connection)).Result) {
                    Logger.Warn("Invalid login username=" + username);
                    Disconnect();
                    return;
                }
            }

            if(!account.SessionId.Equals(sessionid, StringComparison.InvariantCultureIgnoreCase)) {
                Logger.Warn("Invalid sessionid for username=" + username);
                Disconnect();
                return;
            }

            if(!NetUtil.CompareEndPoints(account.EndPoint, RemoteEndPoint)) {
                Logger.Warn("Invalid endpoint for username=" + username);
                Disconnect();
                return;
            }

            Account = account.ToAccount();
        }

        public override void Logout()
        {
            Disconnect();
        }

        public void Ping()
        {
            SendMessage(new PingMessage());
        }
    }
}
