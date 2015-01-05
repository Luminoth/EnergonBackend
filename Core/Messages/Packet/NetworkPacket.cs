using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages.Packet
{
    /*
     * Packet Format:
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

        public async override Task<byte[]> SerializeAsync(IMessageFormatter formatter)
        {
            using(MemoryStream stream = new MemoryStream()) {
                await formatter.WriteAsync(Marker, 0, Marker.Length, stream).ConfigureAwait(false);
                await formatter.WriteIntAsync(Id, stream).ConfigureAwait(false);

                if(!HasContent) {
                    await formatter.WriteStringAsync("null", stream).ConfigureAwait(false);
                    await formatter.WriteIntAsync(0, stream).ConfigureAwait(false);
                } else {
                    await formatter.WriteStringAsync(Content.Type, stream).ConfigureAwait(false);
                    using(MemoryStream mstream = new MemoryStream()) {
                        await Content.SerializeAsync(mstream, formatter).ConfigureAwait(false);

                        byte[] bytes = mstream.ToArray();
                        if(bytes.Length > MaxContentLength) {
                            throw new MessageException("Packet content is too large!");
                        }

                        await formatter.WriteIntAsync(bytes.Length, stream).ConfigureAwait(false);
                        await formatter.WriteAsync(bytes, 0, bytes.Length, stream).ConfigureAwait(false);
                    }
                }

                await formatter.WriteAsync(Terminator, 0, Terminator.Length, stream).ConfigureAwait(false);
                return stream.ToArray();
            }
        }

        public async override Task<bool> DeSerializeAsync(MemoryBuffer buffer, IMessageFormatter formatter)
        {
            byte[] marker = new byte[Marker.Length];
            await buffer.ReadAsync(marker, 0, marker.Length).ConfigureAwait(false);
            if(!marker.SequenceEqual(Marker)) {
                throw new MessageException("Invalid marker!");
            }

            Id = await formatter.ReadIntAsync(buffer.Buffer).ConfigureAwait(false);

            string type = await formatter.ReadStringAsync(buffer.Buffer).ConfigureAwait(false);
            Content = MessageFactory.Create(type);

            int contentLength = await formatter.ReadIntAsync(buffer.Buffer).ConfigureAwait(false);
            if(contentLength > MaxContentLength) {
                throw new MessageException("Invalid packet content length: " + contentLength);
            }

            int expectedLength = contentLength + Terminator.Length;
            if(expectedLength > buffer.Remaining) {
                return false;
            }

            if(null != Content) {
                try {
                    await Content.DeSerializeAsync(buffer.Buffer, formatter).ConfigureAwait(false);
                } catch(Exception e) {
                    throw new MessageException("Exception while deserializing content", e);
                }
            }

            byte[] terminator = new byte[Terminator.Length];
            await buffer.ReadAsync(terminator, 0, terminator.Length).ConfigureAwait(false);
            if(!terminator.SequenceEqual(Terminator)) {
                throw new MessageException("Invalid message terminator!");
            }

            return true;
        }

        public override string ToString()
        {
            return "NetworkMessage(Id: " + Id + ", Content: " + Content + ")";
        }
    }
}
