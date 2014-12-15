using System;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages.Packet
{
    [Serializable]
    public abstract class MessagePacket : IComparable
    {
#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

        public int Id { get; protected set; }

        public IMessage Payload { get; set; }
        public bool HasPayload { get { return null != Payload; } }

        public MessagePacket()
        {
            Id = NextId;
        }

        public abstract byte[] Serialize(IMessageFormatter formatter);
        public abstract bool DeSerialize(MemoryBuffer buffer, IMessageFormatter formatter);

        public int CompareTo(object obj)
        {
            MessagePacket rhs = obj as MessagePacket;
            if(null == rhs) {
                return 0;
            }
            return (int)(Id - rhs.Id);
        }
    }
}
