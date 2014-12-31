using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Packet;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages.Parser
{
    public sealed class NetworkPacketParser : IMessagePacketParser
    {
        public MessagePacket Create()
        {
            return new NetworkPacket();
        }

        public MessagePacket Parse(MemoryBuffer buffer, IMessageFormatter formatter)
        {
            buffer.Flip();
            if(!buffer.HasRemaining) {
                buffer.Reset();
                return null;
            }

            NetworkPacket packet = new NetworkPacket();
            if(!packet.DeSerialize(buffer, formatter)) {
                buffer.Reset();
                return null;
            }

            buffer.Compact();
            return packet;
        }
    }
}
