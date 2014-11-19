using System;
using System.Configuration;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Overmind;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Launcher.Net
{
    sealed class OvermindSession : Session
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(OvermindSession));

        private bool ShouldPing { get {  return 0 == LastMessageTime ? false : Time.CurrentTimeMs > (LastMessageTime + Convert.ToInt64(ConfigurationManager.AppSettings["overmindPingRate"])); } }

        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        public OvermindSession(SessionManager manager) : base(manager)
        {
        }

        public OvermindSession(Socket socket, SessionManager manager) : base(socket, manager)
        {
        }

        protected override void OnRun()
        {
            Ping();
        }

        private void OnConnectFailedCallback(SocketError error)
        {
            Error("Failed to connect to the overmind server: " + error);
        }

        private void OnConnectSuccessCallback()
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
            _logger.Info("Logging in...");

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
            Disconnect();

            ClientState.Instance.LoggingIn = false;
            ClientState.Instance.LoggedIn = false;

            ClientState.Instance.CurrentPage = ClientState.Page.Login;
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
