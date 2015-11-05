using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnergonSoftware.Core.IO;
using EnergonSoftware.Core.Packet;

namespace EnergonSoftware.Backend.Packet
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Binary Packet Format:
    ///     MARKER | ID | ENCODING | CONTENT TYPE | CONTENT LEN | SEPARATOR | CONTENT
    /// </remarks>
    [Serializable]
    public class NetworkPacket : IPacket
    {
#region Id Generator
        private static int _nextId;
        private static int NextId => ++_nextId;
        #endregion

        /// <summary>
        /// The packet type
        /// </summary>
        public const string PacketType = "Network";

        /// <summary>
        /// The packet header (ESNP)
        /// </summary>
        public static readonly byte[] Header = { (byte)'E', (byte)'S', (byte)'N', (byte)'P' };

        /// <summary>
        /// The separator between the packet header and the conten
        /// </summary>
        public static readonly byte[] Separator = { (byte)'\r', (byte)'\n' };

        public string Type => PacketType;

        public int Id { get; protected set; } = NextId;

        public string ContentType { get; set; }

        public string Encoding { get; set; }

        public int ContentLength => Content?.Length ?? 0;

        public byte[] Content { get; set; }

        public async Task SerializeAsync(Stream stream)
        {
            await stream.WriteAsync(Header, 0, Header.Length).ConfigureAwait(false);
            await stream.WriteNetworkAsync(Id).ConfigureAwait(false);
            await stream.WriteNetworkAsync(ContentType).ConfigureAwait(false);
            await stream.WriteNetworkAsync(Encoding).ConfigureAwait(false);
            await stream.WriteNetworkAsync(ContentLength).ConfigureAwait(false);
            await stream.WriteAsync(Separator, 0, Separator.Length).ConfigureAwait(false);
            await stream.WriteAsync(Content, 0, Content.Length).ConfigureAwait(false);
        }

        public async Task<bool> DeserializeAsync(Stream stream)
        {
            // look for the header separator
            long separatorIndex = await stream.IndexOfAsync(Separator).ConfigureAwait(false);
            if(-1 == separatorIndex) {
                return false;
            }

            // read the header values
            byte[] header = new byte[Header.Length];
            await stream.ReadAsync(header, 0, header.Length).ConfigureAwait(false);
            if(!header.SequenceEqual(Header)) {
                throw new PacketException("Invalid packet header!");
            }

            Id = await stream.ReadNetworkIntAsync().ConfigureAwait(false);
            ContentType = await stream.ReadNetworkStringAsync().ConfigureAwait(false);
            Encoding = await stream.ReadNetworkStringAsync().ConfigureAwait(false);
            int contentLength = await stream.ReadNetworkIntAsync().ConfigureAwait(false);

            // make sure we have the entire message
            if(stream.GetRemaining() < Separator.Length + contentLength) {
                return false;
            }

            // consume the separator
            await stream.ConsumeAsync(Separator.Length).ConfigureAwait(false);

            // read the content
            Content = new byte[contentLength];
            await stream.ReadAsync(Content, 0, Content.Length).ConfigureAwait(false);

            return true;
        }

        public int CompareTo(object obj)
        {
            NetworkPacket rhs = obj as NetworkPacket;
            if(null == rhs) {
                return 0;
            }

            return Id - rhs.Id;
        }

        public override bool Equals(object obj)
        {
            NetworkPacket packet = obj as NetworkPacket;
            if(null == packet) {
                return false;
            }

            return Type == packet.Type && Id == packet.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"NetworkPacket(Type: {Type}, Id: {Id}, ContentType: {ContentType}, Encoding: {Encoding}, ContentLength: {ContentLength})";
        }
    }
}
