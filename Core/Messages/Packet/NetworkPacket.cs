using System;
using System.IO;
using System.Linq;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages.Packet
{
    /*
     * Packet Format:
     *      MARKER | ID | TYPE | PAYLOAD LENGTH | PAYLOAD | TERMINATOR
     */
    [Serializable]
    public sealed class NetworkPacket : MessagePacket
    {
        public static readonly byte[] Marker = new byte[] { (byte)'E', (byte)'S', (byte)'N', (byte)'M', 0 };

        private const int MaxPayloadSize = ushort.MaxValue;
        private static readonly byte[] Terminator = new byte[] { (byte)'\r', (byte)'\n', 0 };

        public NetworkPacket()
        {
        }

        public override byte[] Serialize(IMessageFormatter formatter)
        {
            using(MemoryStream stream = new MemoryStream()) {
                formatter.Write(Marker, 0, Marker.Length, stream);
                formatter.WriteInt(Id, stream);

                if(!HasPayload) {
                    formatter.WriteString("null", stream);
                    formatter.WriteInt(0, stream);
                } else {
                    formatter.WriteString(Payload.Type, stream);
                    using(MemoryStream mstream = new MemoryStream()) {
                        Payload.Serialize(mstream, formatter);

                        byte[] bytes = mstream.ToArray();
                        if(bytes.Length > MaxPayloadSize) {
                            throw new MessageException("Packet payload is too large!");
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
            Payload = MessageFactory.CreateMessage(type);

            int payloadLength = formatter.ReadInt(buffer.Buffer);
            if(payloadLength > MaxPayloadSize) {
                throw new MessageException("Invalid packet payload length: " + payloadLength);
            }

            int expectedLength = payloadLength + Terminator.Length;
            if(expectedLength > buffer.Remaining) {
                return false;
            }

            if(null != Payload) {
                try {
                    Payload.DeSerialize(buffer.Buffer, formatter);
                } catch(Exception e) {
                    throw new MessageException("Exception while deserializing payload", e);
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
            return "NetworkMessage(Id: " + Id + ", Payload: " + Payload + ")";
        }
    }
}
