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
    internal sealed class ChatSession : Session
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ChatSession));

        private bool ShouldPing { get { return 0 == LastMessageTime ? false : Time.CurrentTimeMs > (LastMessageTime + Convert.ToInt64(ConfigurationManager.AppSettings["chatPingRate"])); } }

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new MessageHandlerFactory(); } }

        public ChatSession() : base()
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
            SetVisibility(Visibility.Online);
        }

        public void BeginConnect(string host, int port)
        {
            OnConnectSuccess += OnConnectSuccessCallback;
            OnConnectFailed += OnConnectFailedCallback;
            ConnectAsync(host, port);
        }

        public void Ping()
        {
            if(!ShouldPing) {
                return;
            }

            SendMessage(new PingMessage());
        }

        public void SetVisibility(Visibility visibility)
        {
            SendMessage(new VisibilityMessage()
                {
                    Visibility = visibility,
                }
            );
        }
    }
}
