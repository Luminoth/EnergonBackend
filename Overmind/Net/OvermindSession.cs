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

        protected override void LookupAccount(string username)
        {
            Logger.Debug("Looking up account for username=" + username);
            AccountInfo account = new AccountInfo() { Username = username };
            using(DatabaseConnection connection = Task.Run(() => DatabaseManager.AcquireDatabaseConnection()).Result) {
                if(!Task.Run(() => account.Read(connection)).Result) {
                    return;
                }
            }
            Account = account.ToAccount();
        }

        public void Ping()
        {
            SendMessage(new PingMessage());
        }
    }
}
