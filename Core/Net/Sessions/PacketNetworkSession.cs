using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.Packet;

namespace EnergonSoftware.Core.Net.Sessions
{
    /// <summary>
    /// Extends the NetworkSession to work with packets
    /// </summary>
    public abstract class PacketNetworkSession : NetworkSession
    {
        /// <summary>
        /// Sends a packet.
        /// </summary>
        /// <param name="packet">The packet.</param>
        public async Task SendAsync(IPacket packet)
        {
            using(MemoryStream stream = new MemoryStream()) {
                await packet.SerializeAsync(stream).ConfigureAwait(false);
                await SendAsync(stream).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketNetworkSession"/> class.
        /// </summary>
        protected PacketNetworkSession()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketNetworkSession"/> class.
        /// </summary>
        /// <param name="socket">The socket to wrap.</param>
        protected PacketNetworkSession(Socket socket)
            : base(socket)
        {
        }
    }
}
