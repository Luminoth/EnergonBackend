using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;

using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Packet;
using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Net.Sessions
{
    /// <summary>
    /// Extends the PacketNetworkSessionManager to work with messages
    /// </summary>
    public class MessageNetworkSessionManager : PacketNetworkSessionManager
    {
        /// <summary>
        /// Broadcasts the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="formatterType">Type of the formatter.</param>
        /// <param name="packetType">Type of the packet.</param>
        public async Task BroadcastAsync(Message message, string formatterType, string packetType)
        {
            IPacket packet = await MessageNetworkSession.CreatePacketAsync(message, new FormatterFactory().Create(formatterType), packetType).ConfigureAwait(false);
            await BroadcastAsync(packet).ConfigureAwait(false);
        }
    }
}
