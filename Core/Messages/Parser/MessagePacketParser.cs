using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.IO;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Packet;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages.Parser
{
    public interface IMessagePacketParser
    {
        // NOTE: this locks the stream, so do not lock before calling
        Task ParseAsync(IMessageFactory messageFactory);
    }
}
