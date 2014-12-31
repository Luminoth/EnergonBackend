using System;
using System.Configuration;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Chat.MessageHandlers;
using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Models;

using log4net;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class ChatSessionFactory : ISessionFactory
    {
        public Session Create(Socket socket)
        {
            return new ChatSession(socket)
            {
                Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]),
            };
        }
    }

    internal sealed class ChatSession : AuthenticatedSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatSession));

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new ChatMessageHandlerFactory(); } }

        public ChatSession(Socket socket) : base(socket)
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
