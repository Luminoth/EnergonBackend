using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;

using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Serialization;

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
        /// <param name="message">The message.</param>
        /// <param name="formatterType">Type of the formatter.</param>
        /// <param name="packetType">Type of the packet.</param>
        public async Task BroadcastMessageAsync(Message message, string formatterType, string packetType)
        {
            using(MemoryStream packetStream = new MemoryStream()) {
                await MessageSession.SerializeMessageToPacketStreamAsync(message, packetStream, new FormatterFactory().Create(formatterType), packetType).ConfigureAwait(false);
                await BroadcastAsync(packetStream).ConfigureAwait(false);
            }
        }
    }
}
