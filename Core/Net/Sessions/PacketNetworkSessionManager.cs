using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.Packet;

namespace EnergonSoftware.Core.Net.Sessions
{
    /// <summary>
    /// Extends the NetworkSessionManager to work with packets
    /// </summary>
    public class PacketNetworkSessionManager : NetworkSessionManager
    {
        /// <summary>
        /// Broadcasts the packet.
        /// </summary>
        /// <param name="packet">The packet.</param>
        public async Task BroadcastAsync(IPacket packet)
        {
            using(MemoryStream packetStream = new MemoryStream()) {
                await packet.SerializeAsync(packetStream).ConfigureAwait(false);
                await BroadcastAsync(packetStream).ConfigureAwait(false);
            }
        }
    }
}
