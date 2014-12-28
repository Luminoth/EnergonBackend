using System.Net.Sockets;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Overmind.MessageHandlers;

using log4net;

namespace EnergonSoftware.Overmind.Net
{
    internal sealed class InstanceNotifierSession : Session
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(InstanceNotifierSession));

        private readonly SocketState _listener;

        public override IMessagePacketParser Parser { get { return new NetworkPacketParser(); } }
        public override IMessageFormatter Formatter { get { return new BinaryMessageFormatter(); } }
        public override IMessageHandlerFactory HandlerFactory { get { return new InstanceNotifierMessageHandlerFactory(); } }

        private InstanceNotifierSession(Socket socket) : base(socket)
        {
        }

        public InstanceNotifierSession(SocketState listener) : base()
        {
            _listener = listener;
        }

        protected override void OnRun()
        {
            int count = _listener.PollAndRead();
            if(count > 0) {
                Logger.Debug("Instance notifier session " + Id + " read " + count + " bytes");
            }

            Processor.ParseMessages(_listener.Buffer);
        }
    }
}