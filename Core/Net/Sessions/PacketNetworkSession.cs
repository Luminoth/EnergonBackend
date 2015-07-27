using System;
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
#region Events
        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event EventHandler<PacketReceivedEventArgs> PacketReceivedEvent;
#endregion

        /// <summary>
        /// Gets the packet factory.
        /// </summary>
        /// <value>
        /// The packet factory.
        /// </value>
        protected abstract IPacketFactory PacketFactory { get; }

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
        /// Handles the data received event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EnergonSoftware.Core.Net.Sessions.DataReceivedEventArgs" /> instance containing the event data.</param>
        private async void DataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            IPacket packet;

            await SessionReceiveBuffer.LockAsync().ConfigureAwait(false);
            try {
                SessionReceiveBuffer.Flip();

                packet = await new PacketReader().ReadAsync(PacketFactory, SessionReceiveBuffer).ConfigureAwait(false);
                if(null == packet) {
                    SessionReceiveBuffer.Reset();
                    return;
                }

                await SessionReceiveBuffer.CompactAsync().ConfigureAwait(false);
            } finally {
                SessionReceiveBuffer.Release();
            }

            PacketReceivedEvent?.Invoke(this, new PacketReceivedEventArgs
                {
                    Packet = packet
                }
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketNetworkSession"/> class.
        /// </summary>
        protected PacketNetworkSession()
        {
            DataReceivedEvent += DataReceivedEventHandler;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketNetworkSession"/> class.
        /// </summary>
        /// <param name="socket">The socket to wrap.</param>
        protected PacketNetworkSession(Socket socket)
            : base(socket)
        {
            DataReceivedEvent += DataReceivedEventHandler;
        }
    }
}
