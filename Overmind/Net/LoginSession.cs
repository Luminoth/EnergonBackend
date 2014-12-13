using System;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Database.Models;

using log4net;

namespace EnergonSoftware.Overmind.Net
{
    internal sealed class LoginSessionFactory : ISessionFactory
    {
        public Session CreateSession(SessionManager manager)
        {
            return new LoginSession(manager);
        }

        public Session CreateSession(Socket socket, SessionManager manager)
        {
            return new LoginSession(socket, manager);
        }
    }

    internal sealed class LoginSession : Session
    {
        public AccountInfo AccountInfo { get; private set; }

        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public LoginSession(SessionManager manager) : base(manager)
        {
        }

        public LoginSession(Socket socket, SessionManager manager) : base(socket, manager)
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
