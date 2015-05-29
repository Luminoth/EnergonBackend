using System.IO;
using System.Threading.Tasks;

namespace EnergonSoftware.Backend.Packet
{
    public static class PacketReader
    {
        public static async Task<IPacket> ReadAsync(MemoryStream stream)
        {
            IPacket packet = null;

            stream.Flip();
            try {
                if(!stream.HasRemaining()) {
                    return null;
                }

                packet = await PacketFactory.CreateAsync(stream).ConfigureAwait(false);
                if(null == packet) {
                    return null;
                }

                if(!await packet.DeserializeAsync(stream).ConfigureAwait(false)) {
                    packet = null;
                    return null;
                }

                await stream.CompactAsync().ConfigureAwait(false);
                return packet;
            } finally {
                if(null == packet) {
                    stream.Reset();
                }
            }
        }
    }
}
