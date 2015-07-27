using System;

using EnergonSoftware.Core.Packet;

namespace EnergonSoftware.Core.Net.Sessions
{
    /// <summary>
    /// Emitted when a new packet is received
    /// </summary>
    public sealed class PacketReceivedEventArgs
    {
        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        /// <value>
        /// The event timestamp.
        /// </value>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the packet.
        /// </summary>
        /// <value>
        /// The packet.
        /// </value>
        public IPacket Packet { get; set; }
    }
}
