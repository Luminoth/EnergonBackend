using System.IO;
using System.Net.Sockets;
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

            IPacket packet = new BackendPacketFactory().Create(packetType);
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
        /// Gets the type of the message formatter to use.
        /// </summary>
        /// <value>
        /// The type of the message formatter to use.
        /// </value>
        protected abstract string MessageFormatterType { get; }

        /// <summary>
        /// Gets the type of the packet to use.
        /// </summary>
        /// <value>
        /// The type of the packet to use.
        /// </value>
        protected abstract string PacketType { get; }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public async Task SendMessageAsync(Message message)
        {
            try {
                using(MemoryStream packetStream = new MemoryStream()) {
                    await SerializeMessageToPacketStreamAsync(message, packetStream, new FormatterFactory().Create(MessageFormatterType), PacketType).ConfigureAwait(false);
                    await SendAsync(packetStream).ConfigureAwait(false);
                }
            } catch(MessageException e) {
                await InternalErrorAsync(Resources.ErrorSendingMessage, e).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageSession"/> class.
        /// </summary>
        protected MessageSession()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageSession"/> class.
        /// </summary>
        /// <param name="socket">The already connected socket to wrap.</param>
        protected MessageSession(Socket socket)
            : base(socket)
        {
        }
    }
}
