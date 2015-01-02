using System;
using System.Configuration;
using System.Net.Sockets;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Chat;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Launcher.MessageHandlers;

using log4net;

namespace EnergonSoftware.Launcher.Net
{
    internal sealed class OvermindSession : Session
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OvermindSession));

        private bool ShouldPing { get { return 0 == LastMessageTime ? false : Time.CurrentTimeMs > (LastMessageTime + Convert.ToInt64(ConfigurationManager.AppSettings["overmindPingRate"])); } }

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new MessageHandlerFactory(); } }

        public OvermindSession() : base()
        {
        }

        protected override void OnRun()
        {
            Ping();
        }

        private void OnConnectFailedCallback(object sender, ConnectEventArgs e)
        {
            Error("Failed to connect to the overmind server: " + e.Error);
        }

        private void OnConnectSuccessCallback(object sender, ConnectEventArgs e)
        {
            Login();
        }

        public void BeginConnect(string host, int port)
        {
            OnConnectSuccess += OnConnectSuccessCallback;
            OnConnectFailed += OnConnectFailedCallback;
            ConnectAsync(host, port);
        }

        private void Login()
        {
            SendMessage(new LoginMessage()
                {
                    Username = ClientState.Instance.Username,
                    SessionId = ClientState.Instance.Ticket,
                }
            );
        }

        public void Logout()
        {
            SendMessage(new LogoutMessage()
                {
                    Username = ClientState.Instance.Username,
                    SessionId = ClientState.Instance.Ticket,
                }
            );
            Disconnect();
        }

        public void Ping()
        {
            if(!ShouldPing) {
                return;
            }

            SendMessage(new PingMessage());
        }
    }
}
