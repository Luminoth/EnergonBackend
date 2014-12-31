using System;
using System.Configuration;
using System.Net.Sockets;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Overmind.MessageHandlers;


namespace EnergonSoftware.Overmind.Net
{
    internal sealed class OvermindSessionFactory : ISessionFactory
    {
        public Session Create(Socket socket)
        {
            return new OvermindSession(socket)
            {
                Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]),
            };
        }
    }

    internal sealed class OvermindSession : AuthenticatedSession
    {
        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new OvermindMessageHandlerFactory(); } }

        public OvermindSession(Socket socket) : base(socket)
        {
        }

        public void Ping()
        {
            SendMessage(new PingMessage());
        }

        public void Logout()
        {
            Disconnect();
        }
    }
}
