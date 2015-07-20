using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Packet;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Backend.Net.Sessions
{
    /// <summary>
    /// Extends the NetworkSessionManager to work with messages
    /// </summary>
    public class MessageSessionManager : NetworkSessionManager
    {
        /// <summary>
        /// Broadcasts the message.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="formatterType">Type of the formatter.</param>
        public async Task BroadcastMessageAsync(IPacket packet, string formatterType)
        {
            using(MemoryStream buffer = new MemoryStream()) {
                await packet.SerializeAsync(buffer, formatterType).ConfigureAwait(false);
                await BroadcastAsync(buffer).ConfigureAwait(false);
            }
        }
    }
}
