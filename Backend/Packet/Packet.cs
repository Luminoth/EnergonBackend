using System;
using System.IO;
using System.Threading.Tasks;

namespace EnergonSoftware.Backend.Packet
{
    /// <summary>
    /// Represents a packet of data
    /// </summary>
    public interface IPacket : IComparable
    {
        /// <summary>
        /// Gets the packet type.
        /// </summary>
        /// <value>
        /// The packet type.
        /// </value>
        string Type { get; }

        /// <summary>
        /// Gets the packet identifier.
        /// This should be a value suitable for ordering packets.
        /// </summary>
        /// <value>
        /// The packet identifier.
        /// </value>
        int Id { get; }

        /// <summary>
        /// Gets or sets the type of the packet content.
        /// </summary>
        /// <value>
        /// The type of the packet content.
        /// </value>
        string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content encoding.
        /// This is not necessarily a text encoding value.
        /// </summary>
        /// <value>
        /// The content encoding.
        /// </value>
        string Encoding { get; set; }

        /// <summary>
        /// Gets the length of the packet content.
        /// </summary>
        /// <value>
        /// The length of the packet content.
        /// </value>
        int ContentLength { get; }

        /// <summary>
        /// Gets or sets the packet content.
        /// </summary>
        /// <value>
        /// The packet content.
        /// </value>
        byte[] Content { get; set; }

        /// <summary>
        /// Serializes the packet to a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        Task SerializeAsync(Stream stream);

        /// <summary>
        /// Deserializes the packet from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        Task<bool> DeserializeAsync(Stream stream);
    }
}
