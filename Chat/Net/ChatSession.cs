using System;
using System.Configuration;
using System.Net.Sockets;

using EnergonSoftware.Chat.MessageHandlers;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class ChatSessionFactory : ISessionFactory
    {
        public Session Create(Socket socket)
        {
            return new ChatSession(socket)
            {
                Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]),
            };
        }
    }

    internal sealed class ChatSession : AuthenticatedSession
    {
        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new ChatMessageHandlerFactory(); } }

        public ChatSession(Socket socket) : base(socket)
        {
        }

        public void Ping()
        {
            SendMessage(new PingMessage());
        }
    }
}
