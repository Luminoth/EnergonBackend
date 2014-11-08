using System;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Database.Objects;

namespace EnergonSoftware.Overmind.Net
{
    sealed class LoginSessionFactory : ISessionFactory
    {
        public Session CreateSession()
        {
            return new LoginSession();
        }

        public Session CreateSession(Socket socket)
        {
            return new LoginSession(socket);
        }
    }

    sealed class LoginSession : Session
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LoginSession));

        public AccountInfo AccountInfo { get; private set; }

        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public LoginSession() : base()
        {
        }

        public LoginSession(Socket socket) : base(socket)
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
