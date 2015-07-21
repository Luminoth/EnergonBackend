using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Packet;
using EnergonSoftware.Backend.Properties;

using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Packet;
using EnergonSoftware.Core.Serialization;

using log4net;

namespace EnergonSoftware.Backend.Net.Sessions
{
    /// <summary>
    /// Extends the NetworkSession to work with messages
    /// </summary>
    public abstract class MessageSession : NetworkSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageSession));

        internal static async Task SerializeMessageToPacketStreamAsync(Message message, MemoryStream packetStream, IFormatter formatter, string packetType)
        {
            Logger.Debug("Serializing message:");
            Logger.Debug(message.ToString());

            IPacket packet = PacketFactory.Create(packetType);
            using(MemoryStream messageStream = new MemoryStream()) {
                formatter.Attach(messageStream);
                await Message.SerializeAsync(message, formatter).ConfigureAwait(false);

                packet.ContentType = message.ContentType;
                packet.Encoding = formatter.Type;
                packet.Content = messageStream.ToArray();
            }

            Logger.Debug("Serializing packet:");
            Logger.Debug(packet.ToString());
            await packet.SerializeAsync(packetStream).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="formatterType">Type of the formatter.</param>
        /// <param name="packetType">Type of the packet.</param>
        public async Task SendMessageAsync(Message message, string formatterType, string packetType)
        {
            try {
                using(MemoryStream packetStream = new MemoryStream()) {
                    await SerializeMessageToPacketStreamAsync(message, packetStream, FormatterFactory.Create(formatterType), packetType).ConfigureAwait(false);
                    await SendAsync(packetStream).ConfigureAwait(false);
                }
            } catch(MessageException e) {
                await InternalErrorAsync(Resources.ErrorSendingMessage, e).ConfigureAwait(false);
            }
        }
    }
}
