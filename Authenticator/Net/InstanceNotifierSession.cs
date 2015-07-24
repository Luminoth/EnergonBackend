using System.Net.Sockets;

using EnergonSoftware.Backend.Net.Sessions;
using EnergonSoftware.Backend.Packet;
using EnergonSoftware.Core.Serialization.Formatters;

namespace EnergonSoftware.Authenticator.Net
{
    internal sealed class InstanceNotifierSession : MessageSession
    {
        public override string Name => "instanceNotifier";

        protected override string MessageFormatterType => BinaryNetworkFormatter.FormatterType;

        protected override string PacketType => NetworkPacket.PacketType;

// TODO: this needs to override the poll methods or something
// so that we listen to the listener and not the sender

        private Socket _listener;

        public InstanceNotifierSession(Socket listener)
        {
            _listener = listener;
        }
    }
}
