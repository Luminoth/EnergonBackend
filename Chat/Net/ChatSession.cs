using System;
using System.Configuration;
using System.Net.Sockets;

using EnergonSoftware.Chat.MessageHandlers;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class ChatSessionFactory : ISessionFactory
    {
        public Session Create(Socket socket)
        {
            ChatSession session = new ChatSession(socket);
            session.Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
            return session;
        }
    }

    internal sealed class ChatSession : Session
    {
        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new ChatMessageHandlerFactory(); } }

        public ChatSession(Socket socket) : base(socket)
        {
        }
    }
}
