using System.Net.Sockets;
using System.Threading.Tasks;

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
        public override string FormatterType { get { return BinaryMessageFormatter.FormatterType; } }
        protected override IMessageHandlerFactory HandlerFactory { get { return new InstanceNotifierMessageHandlerFactory(); } }

        private InstanceNotifierSession(Socket socket) : base(socket)
        {
        }

        public InstanceNotifierSession(SocketState listener) : base()
        {
            _listener = listener;
        }

        protected async override Task OnRunAsync()
        {
            int count = await _listener.PollAndReadAsync().ConfigureAwait(false);
            if(count > 0) {
                Logger.Debug("Instance notifier session " + Id + " read " + count + " bytes");
            }

            await Processor.ParseMessagesAsync(_listener.Buffer).ConfigureAwait(false);
        }
    }
}