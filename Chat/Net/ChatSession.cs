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
            AccountInfo accountInfo = new AccountInfo() { Username = username };
            using(DatabaseConnection connection = Task.Run(() => DatabaseManager.AcquireDatabaseConnection()).Result) {
                if(!Task.Run(() => accountInfo.Read(connection)).Result) {
                    Logger.Warn("Invalid login username=" + username);
                    Disconnect();
                    return;
                }
            }
            Account = accountInfo.ToAccount();

            EnergonSoftware.Core.Accounts.Account account = new Account() { Username = username, SessionId = sessionid, EndPoint = RemoteEndPoint };
            if(!Authenticate(account)) {
                Logger.Warn("Invalid login account: " + account + ", expected: " + Account);
                Disconnect();
                return;
            }
        }

        public override void Logout()
        {
            Disconnect();

            Account = null;
        }

        public void Ping()
        {
            SendMessage(new PingMessage());
        }
    }
}
