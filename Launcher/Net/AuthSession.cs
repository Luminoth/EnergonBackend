﻿using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Auth;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net.Sessions;
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
        public override string FormatterType { get { return BinaryMessageFormatter.FormatterType; } }
        protected override IMessageHandlerFactory HandlerFactory { get { return new MessageHandlerFactory(); } }

        public AuthSession() : base()
        {
            AuthStage = AuthenticationStage.NotAuthenticated;
        }

        public async Task BeginConnectAsync(string host, int port)
        {
            try {
                await ConnectAsync(host, port, SocketType.Stream, ProtocolType.Tcp).ConfigureAwait(false);
                if(!Connected) {
                    await ErrorAsync("Failed to connect to the chat server").ConfigureAwait(false);
                    return;
                }

                await BeginAuthAsync().ConfigureAwait(false);
            } catch(SocketException e) {
                AuthFailedAsync("Failed to connect to the authentication server: " + e.Message).Wait();
            }
        }

        private async Task BeginAuthAsync()
        {
            Logger.Info("Authenticating as user '" + ClientState.Instance.Username + "'...");

            await SendMessageAsync(new AuthMessage()
                {
                    MechanismType = AuthType.DigestSHA512,
                }
            ).ConfigureAwait(false);

            AuthStage = AuthenticationStage.Begin;
        }

        public async Task AuthResponseAsync(string response, string rspAuth)
        {
            RspAuth = rspAuth;

            await SendMessageAsync(new ResponseMessage()
                {
                    Response = response,
                }
            ).ConfigureAwait(false);

            AuthStage = AuthenticationStage.Challenge;
        }

        public async Task AuthFinalizeAsync()
        {
            await SendMessageAsync(new ResponseMessage()).ConfigureAwait(false);
            AuthStage = AuthenticationStage.Finalize;
        }

        public async Task AuthSuccessAsync(string ticket)
        {
            Logger.Info("Authentication successful!");
            Logger.Debug("Ticket=" + ticket);

            AuthStage = AuthenticationStage.Authenticated;
            ClientState.Instance.Ticket = ticket;
            ClientState.Instance.Password = null;

            if(null != OnAuthSuccess) {
                OnAuthSuccess();
            }

            await DisconnectAsync().ConfigureAwait(false);
        }

        public async Task AuthFailedAsync(string reason)
        {
            Logger.Warn("Authentication failed: " + reason);

            AuthStage = AuthenticationStage.NotAuthenticated;
            ClientState.Instance.Password = null;

            if(null != OnAuthFailed) {
                OnAuthFailed(reason);
            }

            await DisconnectAsync().ConfigureAwait(false);
        }
    }
}
