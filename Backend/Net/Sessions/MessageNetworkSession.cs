using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using EnergonSoftware.Backend.MessageHandlers;
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
    /// Extends the PacketNetworkSession to work with messages
    /// </summary>
    public abstract class MessageNetworkSession : PacketNetworkSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageNetworkSession));

        internal static async Task<IPacket> CreatePacketAsync(Message message, IFormatter formatter, string packetType)
        {
            Logger.Debug("Serializing message:");
            Logger.Debug(message.ToString());

            IPacket packet = new BackendPacketFactory().Create(packetType);
            using(MemoryStream stream = new MemoryStream()) {
                formatter.Attach(stream);
                await Message.SerializeAsync(message, formatter).ConfigureAwait(false);

                packet.ContentType = message.ContentType;
                packet.Encoding = formatter.Type;
                packet.Content = stream.ToArray();

                return packet;
            }
        }

#region Events
        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;
#endregion

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
        /// Gets the message handler factory.
        /// </summary>
        /// <value>
        /// The message handler factory.
        /// </value>
        public abstract IMessageHandlerFactory MessageHandlerFactory { get; }

        protected override IPacketFactory PacketFactory => new BackendPacketFactory();

        /// <summary>
        /// Gets the message factory.
        /// </summary>
        /// <value>
        /// The message factory.
        /// </value>
        protected virtual IMessageFactory MessageFactory => new BackendMessageFactory();

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public async Task SendAsync(Message message)
        {
            try {
                IPacket packet = await CreatePacketAsync(message, new FormatterFactory().Create(MessageFormatterType), PacketType).ConfigureAwait(false);
                await SendAsync(packet).ConfigureAwait(false);
            } catch(MessageException e) {
                await InternalErrorAsync(Resources.ErrorSendingMessage, e).ConfigureAwait(false);
            }
        }

        private async void PacketReceivedEventHandler(object sender, PacketReceivedEventArgs e)
        {
            if(!Message.IsMessageContentType(e.Packet.ContentType)) {
                await ErrorAsync("Packet content is not a message!").ConfigureAwait(false);
                return;
            }

// TODO: exceptions here?

            Logger.Debug("Attempting to parse a message...");
            Message message = await Message.DeSerializeAsync(e.Packet.ContentType, e.Packet.Encoding, e.Packet.Content, 0, e.Packet.ContentLength, MessageFactory).ConfigureAwait(false);
            if(null == message) {
                await ErrorAsync("Failed to parse a message!").ConfigureAwait(false);
                return;
            }

            Logger.Debug($"{message.Type} message parsed!");
            MessageReceivedEvent?.Invoke(this, new MessageReceivedEventArgs
                {
                    Message = message
                }
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNetworkSession"/> class.
        /// </summary>
        protected MessageNetworkSession()
        {
            PacketReceivedEvent += PacketReceivedEventHandler;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNetworkSession"/> class.
        /// </summary>
        /// <param name="socket">The socket to wrap.</param>
        protected MessageNetworkSession(Socket socket)
            : base(socket)
        {
            PacketReceivedEvent += PacketReceivedEventHandler;
        }
    }
}
