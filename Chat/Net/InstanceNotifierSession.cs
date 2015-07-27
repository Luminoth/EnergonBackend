using System.Net.Sockets;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Net.Sessions;
using EnergonSoftware.Backend.Packet;

using EnergonSoftware.Chat.MessageHandlers;

using EnergonSoftware.Core.Serialization.Formatters;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class InstanceNotifierSession : MessageNetworkSession
    {
        public override string Name => "instanceNotifier";

        // TODO: make this configurable
        public override int MaxSessionReceiveBufferSize => 1024 * 1000 * 10;

        protected override string MessageFormatterType => BinaryNetworkFormatter.FormatterType;

        protected override string PacketType => NetworkPacket.PacketType;

        public override IMessageHandlerFactory MessageHandlerFactory => new InstanceNotifierMessageHandlerFactory();

// TODO: this needs to override the poll methods or something
// so that we listen to the listener and not the sender

        private Socket _listener;

        public InstanceNotifierSession(Socket listener)
        {
            _listener = listener;
        }
    }
}
