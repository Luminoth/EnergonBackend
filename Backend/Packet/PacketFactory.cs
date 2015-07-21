using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.Packet;

namespace EnergonSoftware.Backend.Packet
{
    /// <summary>
    /// Creates packets.
    /// </summary>
    public static class PacketFactory
    {
        /// <summary>
        /// Creates a packet of the specified type.
        /// </summary>
        /// <param name="type">The packet type.</param>
        /// <returns>A new packet of the specified type.</returns>
        /// <exception cref="System.ArgumentException">Invalid packet type</exception>
        public static IPacket Create(string type)
        {
            switch(type)
            {
            case NetworkPacket.PacketType:
                return new NetworkPacket();
            }

            throw new ArgumentException("Invalid packet type", nameof(type));
        }

        /// <summary>
        /// Attempts to deduce the packet type from a stream and create a packet of that type.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>A new packet of the deduced type or null if the type couldn't be deduced</returns>
        public static async Task<IPacket> CreateAsync(Stream stream)
        {
            if(stream.GetRemaining() >= NetworkPacket.Header.Length) {
                byte[] header = new byte[NetworkPacket.Header.Length];
                await stream.PeekAsync(header, 0, header.Length).ConfigureAwait(false);
                if(header.SequenceEqual(NetworkPacket.Header)) {
                    return new NetworkPacket();
                }
            }

            return null;
        }
    }
}
