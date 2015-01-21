using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Packet;
using EnergonSoftware.Core.Properties;
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

        public async Task<MessagePacket> ParseAsync(MemoryStream stream)
        {
            stream.Flip();
            if(!stream.HasRemaining()) {
                stream.Reset();
                return null;
            }

            NetworkPacket packet = new NetworkPacket();
            try {
                await packet.DeSerializeAsync(stream).ConfigureAwait(false);
            } catch(MessageException e) {
                Logger.Warn(Resources.ErrorParsingNetworkPacket, e);
                stream.Reset();
                return null;
            }

            await stream.CompactAsync().ConfigureAwait(false);
            return packet;
        }
    }
}
