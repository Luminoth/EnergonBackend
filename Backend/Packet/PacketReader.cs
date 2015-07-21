using System.IO;
using System.Threading.Tasks;

namespace EnergonSoftware.Backend.Packet
{
    /// <summary>
    /// Reads packets from a stream.
    /// </summary>
    public class PacketReader
    {
        /// <summary>
        /// Reads a single packet from a string.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// The parsed packet.
        /// Returns null if a full packet could not be read.
        /// </returns>
        public async Task<IPacket> ReadAsync(MemoryStream stream)
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
