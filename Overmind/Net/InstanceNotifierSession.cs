﻿using System.Net.Sockets;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;

using log4net;

namespace EnergonSoftware.Overmind.Net
{
    internal sealed class InstanceNotifierSession : Session
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(InstanceNotifierSession));

        private readonly SocketState _listener;

        protected override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }

        private InstanceNotifierSession(SessionManager manager) : base(manager)
        {
        }

        private InstanceNotifierSession(Socket socket, SessionManager manager) : base(socket, manager)
        {
        }

        public InstanceNotifierSession(SessionManager manager, SocketState listener) : base(manager)
        {
            _listener = listener;
        }

        protected override void OnRun(MessageProcessor processor, IMessageParser parser)
        {
            int count = _listener.PollAndRead();
            if(count > 0) {
                Logger.Debug("Instance notifier session " + Id + " read " + count + " bytes");
            }

            processor.ParseMessages(this, parser, _listener.Buffer, Formatter);
        }
    }
}