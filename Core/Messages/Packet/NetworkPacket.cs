using System;
using System.IO;
using System.Linq;

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

        public override byte[] Serialize(IMessageFormatter formatter)
        {
            using(MemoryStream stream = new MemoryStream()) {
                formatter.Write(Marker, 0, Marker.Length, stream);
                formatter.WriteInt(Id, stream);

                if(!HasContent) {
                    formatter.WriteString("null", stream);
                    formatter.WriteInt(0, stream);
                } else {
                    formatter.WriteString(Content.Type, stream);
                    using(MemoryStream mstream = new MemoryStream()) {
                        Content.Serialize(mstream, formatter);

                        byte[] bytes = mstream.ToArray();
                        if(bytes.Length > MaxContentLength) {
                            throw new MessageException("Packet content is too large!");
                        }

                        formatter.WriteInt(bytes.Length, stream);
                        formatter.Write(bytes, 0, bytes.Length, stream);
                    }
                }

                formatter.Write(Terminator, 0, Terminator.Length, stream);
                return stream.ToArray();
            }
        }

        public override bool DeSerialize(MemoryBuffer buffer, IMessageFormatter formatter)
        {
            byte[] marker = new byte[Marker.Length];
            buffer.Read(marker, 0, marker.Length);
            if(!marker.SequenceEqual(Marker)) {
                throw new MessageException("Invalid marker!");
            }

            Id = formatter.ReadInt(buffer.Buffer);

            string type = formatter.ReadString(buffer.Buffer);
            Content = MessageFactory.Create(type);

            int contentLength = formatter.ReadInt(buffer.Buffer);
            if(contentLength > MaxContentLength) {
                throw new MessageException("Invalid packet content length: " + contentLength);
            }

            int expectedLength = contentLength + Terminator.Length;
            if(expectedLength > buffer.Remaining) {
                return false;
            }

            if(null != Content) {
                try {
                    Content.DeSerialize(buffer.Buffer, formatter);
                } catch(Exception e) {
                    throw new MessageException("Exception while deserializing content", e);
                }
            }

            byte[] terminator = new byte[Terminator.Length];
            buffer.Read(terminator, 0, terminator.Length);
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
