using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Properties;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages.Packet
{
    /*
     * Binary Packet Format:
     *      MARKER | ID | TYPE | CONTENT LENGTH | CONTENT | TERMINATOR
     */
    [Serializable]
    public sealed class NetworkPacket : MessagePacket
    {
        public static readonly byte[] Marker = new byte[] { (byte)'E', (byte)'S', (byte)'N', (byte)'M', 0 };
        private static readonly byte[] Terminator = new byte[] { (byte)'\r', (byte)'\n', 0 };

        public const string PacketType = "network";
        public override string Type { get { return PacketType; } }

        public async override Task SerializeAsync(Stream stream, string formatterType)
        {
            await stream.WriteAsync(Marker, 0, Marker.Length).ConfigureAwait(false);
            await stream.WriteNetworkAsync(Id).ConfigureAwait(false);

            await stream.WriteNetworkAsync(formatterType).ConfigureAwait(false);
            IMessageFormatter formatter = MessageFormatterFactory.Create(formatterType);
            formatter.Attach(stream);

            await stream.WriteNetworkAsync(null == Content ? "null" : Content.Type).ConfigureAwait(false);
            await SerializeContentAsync(stream, formatter).ConfigureAwait(false);

            await stream.WriteAsync(Terminator, 0, Terminator.Length).ConfigureAwait(false);
        }

        public async override Task DeSerializeAsync(Stream stream)
        {
            byte[] marker = new byte[Marker.Length];
            await stream.ReadAsync(marker, 0, marker.Length).ConfigureAwait(false);
            if(!marker.SequenceEqual(Marker)) {
                throw new MessageException(Resources.ErrorInvalidNetworkPacketMarker);
            }

            Id = await stream.ReadNetworkIntAsync().ConfigureAwait(false);

            string formatterType = await stream.ReadNetworkStringAsync().ConfigureAwait(false);
            IMessageFormatter formatter = MessageFormatterFactory.Create(formatterType);
            formatter.Attach(stream);

            string messageType = await stream.ReadNetworkStringAsync().ConfigureAwait(false);
            Content = MessageFactory.Create(messageType);
            await DeSerializeContentAsync(formatter).ConfigureAwait(false);

            byte[] terminator = new byte[Terminator.Length];
            await stream.ReadAsync(terminator, 0, terminator.Length).ConfigureAwait(false);
            if(!terminator.SequenceEqual(Terminator)) {
                throw new MessageException(Resources.ErrorInvalidNetworkPacketTerminator);
            }
        }

        public override string ToString()
        {
            return "NetworkMessage(Id: " + Id + ", Content: " + Content + ")";
        }
    }
}
