using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Packet;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.Messages.Parser
{
    public sealed class NetworkPacketParser : IMessagePacketParser
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NetworkPacketParser));

        public MessagePacket Create()
        {
            return new NetworkPacket();
        }

        public async Task<MessagePacket> ParseAsync(MemoryBuffer buffer, string formatterType)
        {
            buffer.Flip();
            if(!buffer.HasRemaining) {
                buffer.Reset();
                return null;
            }

            IMessageFormatter formatter = MessageFormatterFactory.Create(formatterType);
            formatter.Attach(buffer.Buffer);

            NetworkPacket packet = new NetworkPacket();
            try {
                await packet.DeSerializeAsync(formatter).ConfigureAwait(false);
            } catch(MessageException) {
                buffer.Reset();
                return null;
            }

            await buffer.CompactAsync().ConfigureAwait(false);
            return packet;
        }
    }
}
