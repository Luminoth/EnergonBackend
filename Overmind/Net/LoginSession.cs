using System;
using System.Configuration;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Database.Models;
using EnergonSoftware.Overmind.MessageHandlers;

using log4net;

namespace EnergonSoftware.Overmind.Net
{
    internal sealed class LoginSessionFactory : ISessionFactory
    {
        public Session Create(Socket socket)
        {
            LoginSession session = new LoginSession(socket);
            session.Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
            return session;
        }
    }

    internal sealed class LoginSession : Session
    {
        public AccountInfo AccountInfo { get; private set; }

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new OvermindMessageHandlerFactory(); } }

        public LoginSession(Socket socket) : base(socket)
        {
        }

        public void Ping()
        {
            PingMessage ping = new PingMessage();
            SendMessage(ping);
        }

        public async Task LoginFailure(string username, string reason)
        {
            await EventLogger.Instance.LoginFailedEvent(RemoteEndPoint, username, reason);

            Disconnect();
        }

        public async Task LoginSuccess(AccountInfo account)
        {
            InstanceNotifier.Instance.Login(account.Username, RemoteEndPoint);
            await EventLogger.Instance.LoginSuccessEvent(RemoteEndPoint, account.Username);

            AccountInfo = account;
        }

        public async Task Logout()
        {
            InstanceNotifier.Instance.Logout(AccountInfo.Username);
            await EventLogger.Instance.LogoutEvent(RemoteEndPoint, AccountInfo.Username);

            Disconnect();
        }
    }
}
