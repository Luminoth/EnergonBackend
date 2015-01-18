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

        private const int MaxContentLength = ushort.MaxValue;
        private static readonly byte[] Terminator = new byte[] { (byte)'\r', (byte)'\n', 0 };

        public const string PacketType = "network";
        public override string Type { get { return PacketType; } }

        public async override Task SerializeAsync(IMessageFormatter formatter)
        {
            await formatter.WriteAsync(Marker, 0, Marker.Length).ConfigureAwait(false);
            await formatter.WriteAsync("id", Id).ConfigureAwait(false);

            if(null == Content) {
                await formatter.WriteAsync("contentType", "null").ConfigureAwait(false);
            } else {
                await formatter.WriteAsync("contentType", Content.Type).ConfigureAwait(false);
                await Content.SerializeAsync(formatter).ConfigureAwait(false);
            }

            await formatter.WriteAsync(Terminator, 0, Terminator.Length).ConfigureAwait(false);
        }

        public async override Task DeSerializeAsync(IMessageFormatter formatter)
        {
            byte[] marker = new byte[Marker.Length];
            await formatter.ReadAsync(marker, 0, marker.Length).ConfigureAwait(false);
            if(!marker.SequenceEqual(Marker)) {
                throw new MessageException(Resources.ErrorInvalidNetworkPacketMarker);
            }

            Id = await formatter.ReadIntAsync("id").ConfigureAwait(false);

            string type = await formatter.ReadStringAsync("contentType").ConfigureAwait(false);
            Content = MessageFactory.Create(type);

            if(null != Content) {
                try {
                    await Content.DeSerializeAsync(formatter).ConfigureAwait(false);
                } catch(Exception e) {
                    throw new MessageException(Resources.ErrorDeserializingNetworkPacketContent, e);
                }
            }

            byte[] terminator = new byte[Terminator.Length];
            await formatter.ReadAsync(terminator, 0, terminator.Length).ConfigureAwait(false);
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
