﻿using System.Net.Sockets;

using EnergonSoftware.Core;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Launcher.MessageHandlers;

using log4net;

namespace EnergonSoftware.Launcher.Net
{
    internal enum AuthenticationStage
    {
        NotAuthenticated,
        Begin,
        Challenge,
        Finalize,
        Authenticated,
    }

    internal sealed class AuthSession : Session
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuthSession));

#region Events
        public delegate void OnAuthSuccessHandler();
        public event OnAuthSuccessHandler OnAuthSuccess;

        public delegate void OnAuthFailedHandler(string reason);
        public event OnAuthFailedHandler OnAuthFailed;
#endregion

        public AuthenticationStage AuthStage { get; private set; }
        public bool Authenticating { get { return AuthStage > AuthenticationStage.NotAuthenticated && AuthStage < AuthenticationStage.Authenticated; } }
        public bool Authenticated { get { return AuthenticationStage.Authenticated == AuthStage; } }

        public string RspAuth { get; private set; }

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new MessageHandlerFactory(); } }

        public AuthSession() : base()
        {
            AuthStage = AuthenticationStage.NotAuthenticated;
        }

        private void OnConnectFailedCallback(object sender, ConnectEventArgs e)
        {
            AuthFailed("Failed to connect to the authentication server: " + e.Error);
        }

        private void OnConnectSuccessCallback(object sender, ConnectEventArgs e)
        {
            BeginAuth();
        }

        public void BeginConnect(string host, int port)
        {
            OnConnectSuccess += OnConnectSuccessCallback;
            OnConnectFailed += OnConnectFailedCallback;
            ConnectAsync(host, port);
        }

        private void BeginAuth()
        {
            Logger.Info("Authenticating as user '" + ClientState.Instance.Username + "'...");

            SendMessage(new AuthMessage()
                {
                    MechanismType = AuthType.DigestSHA512,
                }
            );
            AuthStage = AuthenticationStage.Begin;
        }

        public void AuthResponse(string response, string rspAuth)
        {
            RspAuth = rspAuth;

            SendMessage(new ResponseMessage()
                {
                    Response = response,
                }
            );
            AuthStage = AuthenticationStage.Challenge;
        }

        public void AuthFinalize()
        {
            SendMessage(new ResponseMessage());
            AuthStage = AuthenticationStage.Finalize;
        }

        public void AuthSuccess(string ticket)
        {
            Logger.Info("Authentication successful!");
            Logger.Debug("Ticket=" + ticket);

            AuthStage = AuthenticationStage.Authenticated;
            ClientState.Instance.Ticket = ticket;
            ClientState.Instance.Password = null;

            if(null != OnAuthSuccess) {
                OnAuthSuccess();
            }

            Disconnect();
        }

        public void AuthFailed(string reason)
        {
            Logger.Warn("Authentication failed: " + reason);

            AuthStage = AuthenticationStage.NotAuthenticated;
            ClientState.Instance.Password = null;

            if(null != OnAuthFailed) {
                OnAuthFailed(reason);
            }

            Disconnect();
        }
    }
}
