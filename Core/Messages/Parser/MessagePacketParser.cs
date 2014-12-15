using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Packet;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages.Parser
{
    public interface IMessagePacketParser
    {
        MessagePacket Parse(MemoryBuffer buffer, IMessageFormatter formatter);
    }
}
