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

        private Socket _listener;

        public InstanceNotifierSession(Socket listener)
        {
            _listener = listener;
        }
    }
}
