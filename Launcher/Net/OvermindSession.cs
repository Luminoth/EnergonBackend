using System;
using System.Configuration;
using System.Net.Sockets;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Launcher.Net
{
    internal sealed class OvermindSession : Session
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OvermindSession));

        private bool ShouldPing { get { return 0 == LastMessageTime ? false : Time.CurrentTimeMs > (LastMessageTime + Convert.ToInt64(ConfigurationManager.AppSettings["overmindPingRate"])); } }

        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public OvermindSession(SessionManager manager) : base(manager)
        {
        }

        private OvermindSession(Socket socket, SessionManager manager) : base(socket, manager)
        {
        }

        protected override void OnRun(MessageProcessor processor, IMessageParser parser)
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
            Logger.Info("Logging in...");

            LoginMessage message = new LoginMessage();
            message.Username = ClientState.Instance.Username;
            message.Ticket = ClientState.Instance.Ticket;
            SendMessage(message);

            ClientState.Instance.LoggingIn = false;
            ClientState.Instance.LoggedIn = true;

            ClientState.Instance.CurrentPage = ClientState.Page.Main;
        }

        public void Logout()
        {
            LogoutMessage message = new LogoutMessage();
            SendMessage(message);
        }

        public void Ping()
        {
            if(!ShouldPing) {
                return;
            }

            PingMessage message = new PingMessage();
            SendMessage(message);
        }
    }
}
