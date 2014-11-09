﻿using System;
using System.Net.Sockets;
using System.Text;

using log4net;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects;

namespace EnergonSoftware.Authenticator.Net
{
    sealed class AuthSessionFactory : ISessionFactory
    {
        public Session CreateSession()
        {
            return new AuthSession();
        }

        public Session CreateSession(Socket socket)
        {
            return new AuthSession(socket);
        }
    }

    sealed class AuthSession : Session
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuthSession));

        public AuthType AuthType { get; private set; }
        public Nonce AuthNonce { get; private set; }

        public bool Authenticating { get; private set; }
        public bool Authenticated { get; private set; }

        public AccountInfo AccountInfo { get; private set; }

        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public AuthSession() : base()
        {
            AuthType = AuthType.None;
        }

        public AuthSession(Socket socket) : base(socket)
        {
            AuthType = AuthType.None;
        }

        protected override void OnRun()
        {
        }

        public void Challenge(AuthType type, Nonce nonce, string challenge)
        {
            AuthType = type;
            AuthNonce = nonce;

            Authenticating = true;
            Authenticated = false;

            ChallengeMessage message = new ChallengeMessage();
            message.Challenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(challenge));
            SendMessage(message);
        }

        public void Challenge(string challenge, AccountInfo accountInfo)
        {
            InstanceNotifier.Instance.Authenticating(accountInfo.Username, RemoteEndPoint);

            AccountInfo = accountInfo;
            Authenticated = true;

            ChallengeMessage message = new ChallengeMessage();
            message.Challenge = Convert.ToBase64String(Encoding.UTF8.GetBytes(challenge));
            SendMessage(message);
        }

        public void Success(string sessionid)
        {
            InstanceNotifier.Instance.Authenticated(AccountInfo.Username, sessionid, RemoteEndPoint);
            EventLogger.Instance.SuccessEvent(RemoteEndPoint, AccountInfo.Username);

            AccountInfo.SessionId = sessionid;
            AccountInfo.SessionEndPoint = RemoteEndPoint.ToString();

            using(DatabaseConnection connection = DatabaseManager.AcquireDatabaseConnection()) {
                AccountInfo.Update(connection);
            }

            SuccessMessage message = new SuccessMessage();
            message.SessionId = sessionid;
            SendMessage(message);

            Disconnect();
        }

        public void Failure(string reason)
        {
            EventLogger.Instance.FailedEvent(RemoteEndPoint, null == AccountInfo ? null : AccountInfo.Username, reason);

            // NOTE: we don't update the database here because this
            // might be a malicious login attempt against an account
            // that is already using a legitimate ticket

            AccountInfo = null;

            FailureMessage message = new FailureMessage();
            message.Reason = reason;
            SendMessage(message);

            Disconnect();
        }
    }
}