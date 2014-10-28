using System;
using System.IO;
using System.Linq;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Net
{
    /*
     * Packet Format:
     *      ID | TYPE | PAYLOAD LENGTH | PAYLOAD | TERMINATOR
     */
    public class NetworkMessage : IComparable
    {
#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

        private static byte[] TERMINATOR = new byte[]{ (byte)'\r', (byte)'\n', 0 };
        private static int MAX_PAYLOAD_SIZE { get { return ushort.MaxValue; } }

        // returns null if not enough data was available
        // throws MessageException if deserialization failed
        public static NetworkMessage Parse(MemoryBuffer buffer, IMessageFormatter formatter)
        {
            buffer.Flip();
            if(!buffer.HasRemaining) {
                buffer.Reset();
                return null;
            }

            NetworkMessage packet = new NetworkMessage();
            if(!packet.DeSerialize(buffer, formatter)) {
                buffer.Reset();
                return null;
            }

            buffer.Compact();
            return packet;
        }

        public int Id { get; private set; }
        public IMessage Payload { get; set; }
        public bool HasPayload { get { return null != Payload; } }

        public NetworkMessage()
        {
            Id = NextId;
        }

        public byte[] Serialize(IMessageFormatter formatter)
        {
            using(MemoryStream stream = new MemoryStream()) {
                formatter.WriteInt(Id, stream);

                if(!HasPayload) {
                    formatter.WriteString("null", stream);
                    formatter.WriteInt(0, stream);
                } else {
                    formatter.WriteString(Payload.Type, stream);
                    using(MemoryStream mstream = new MemoryStream()) {
                        Payload.Serialize(mstream, formatter);

                        byte[] bytes = mstream.ToArray();
                        if(bytes.Length > MAX_PAYLOAD_SIZE) {
                            throw new MessageException("Packet payload is too large!");
                        }

                        formatter.WriteInt(bytes.Length, stream);
                        formatter.Write(bytes, 0, bytes.Length, stream);
                    }
                }

                formatter.Write(TERMINATOR, 0, TERMINATOR.Length, stream);
                return stream.ToArray();
            }
        }

        // returns false if not enough data was available
        // throws MessageException if deserialization failed
        public bool DeSerialize(MemoryBuffer buffer, IMessageFormatter formatter)
        {
            Id = formatter.ReadInt(buffer.Buffer);

            string type = formatter.ReadString(buffer.Buffer);
            Payload = MessageFactory.CreateMessage(type);

            int payloadLength = formatter.ReadInt(buffer.Buffer);
            if(payloadLength > MAX_PAYLOAD_SIZE) {
                throw new MessageException("Invalid packet payload length: " + payloadLength);
            }

            int expectedLength = payloadLength + TERMINATOR.Length;
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

            byte[] terminator = new byte[TERMINATOR.Length];
            buffer.Read(terminator, 0, terminator.Length);
            if(!terminator.SequenceEqual(TERMINATOR)) {
                throw new MessageException("Invalid message terminator!");
            }

            return true;
        }

        public int CompareTo(object obj)
        {
            NetworkMessage rhs = obj as NetworkMessage;
            if(null == rhs) {
                return 0;
            }
            return (int)(Id - rhs.Id);
        }
    }
}
