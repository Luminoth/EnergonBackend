using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages.Parser
{
    public interface IMessageParser
    {
        MessagePacket Parse(MemoryBuffer buffer, IMessageFormatter formatter);
    }
}
