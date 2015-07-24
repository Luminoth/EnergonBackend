using System.IO;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Packet
{
    /// <summary>
    /// Factory for creating packets
    /// </summary>
    public interface IPacketFactory
    {
        /// <summary>
        /// Creates a packte of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A packet of the specified type</returns>
        IPacket Create(string type);

        /// <summary>
        /// Creates a packet through examining the contents of a strema.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// A packet of a type deduced from the contents of the stream.
        /// </returns>
        Task<IPacket> CreateAsync(Stream stream);
    }
}
