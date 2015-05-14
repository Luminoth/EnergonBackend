using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Properties;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Packet
{
    /*
     * Binary Packet Format:
     *      MARKER | ID | ENCODING | CONTENT TYPE | CONTENT LEN | CONTENT
     */
    [Serializable]
    public sealed class NetworkPacket : MessagePacket
    {
        public static readonly byte[] Marker = new byte[] { (byte)'E', (byte)'S', (byte)'N', (byte)'M', 0 };

        public const string PacketType = "network";
        public override string Type { get { return PacketType; } }

        public async override Task SerializeAsync(Stream stream, string formatterType)
        {
            // header values
            await stream.WriteAsync(Marker, 0, Marker.Length).ConfigureAwait(false);
            await stream.WriteNetworkAsync(Id).ConfigureAwait(false);
            await stream.WriteNetworkAsync(formatterType).ConfigureAwait(false);
            await stream.WriteNetworkAsync(null == Content ? "null" : Content.Type).ConfigureAwait(false);

            // serialize content to a separate stream so we can get the length of it
            using(MemoryStream contentStream = new MemoryStream()) {
                IFormatter formatter = FormatterFactory.Create(formatterType);
                formatter.Attach(contentStream);
                await SerializeContentAsync(formatter).ConfigureAwait(false);

                byte[] content = contentStream.ToArray();
                await stream.WriteNetworkAsync(content.Length).ConfigureAwait(false);
                await stream.WriteAsync(content, 0, content.Length).ConfigureAwait(false);
            }
        }

        public async override Task DeSerializeAsync(Stream stream, IMessageFactory messageFactory)
        {
            // header values
            byte[] marker = new byte[Marker.Length];
            await stream.ReadAsync(marker, 0, marker.Length).ConfigureAwait(false);
            if(!marker.SequenceEqual(Marker)) {
                throw new MessageException(Resources.ErrorInvalidNetworkPacketMarker);
            }

            Id = await stream.ReadNetworkIntAsync().ConfigureAwait(false);

            string formatterType = await stream.ReadNetworkStringAsync().ConfigureAwait(false);
            string contentType = await stream.ReadNetworkStringAsync().ConfigureAwait(false);

            int contentLen = await stream.ReadNetworkIntAsync().ConfigureAwait(false);
            if(stream.Length < contentLen) {
                // TODO: handle this better
                throw new MessageException(Resources.ErrorIncompleteNetworkPacket);
            }

            // read the content as a set of bytes
            byte[] content = new byte[contentLen];
            await stream.ReadAsync(content, 0, content.Length).ConfigureAwait(false);

            // serialize the content to a separate stream to match the original serialize process
            // this is very likely superfluous, but keeps the process consistent between serialize/deserialize
            using(MemoryStream contentStream = new MemoryStream()) {
                await contentStream.WriteAsync(content, 0, content.Length);

                IFormatter formatter = FormatterFactory.Create(formatterType);
                formatter.Attach(contentStream);

                Content = messageFactory.Create(contentType);
                await DeSerializeContentAsync(formatter).ConfigureAwait(false);
            }
        }

        public override string ToString()
        {
            return "NetworkMessage(Id: " + Id + ", Content: " + Content + ")";
        }
    }
}
