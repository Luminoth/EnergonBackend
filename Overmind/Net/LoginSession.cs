using System;
using System.Net.Sockets;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Database.Objects;

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

        protected override void OnRun()
        {
        }

        public void Ping()
        {
            PingMessage ping = new PingMessage();
            SendMessage(ping);
        }

        public void LoginFailure(string username, string reason)
        {
            EventLogger.Instance.LoginFailedEvent(RemoteEndPoint, username, reason);

            Disconnect();
        }

        public void LoginSuccess(AccountInfo account)
        {
            InstanceNotifier.Instance.Login(account.Username, RemoteEndPoint);
            EventLogger.Instance.LoginSuccessEvent(RemoteEndPoint, account.Username);

            AccountInfo = account;
        }

        public void Logout()
        {
            InstanceNotifier.Instance.Logout(AccountInfo.Username);
            EventLogger.Instance.LogoutEvent(RemoteEndPoint, AccountInfo.Username);

            Disconnect();
        }
    }
}
