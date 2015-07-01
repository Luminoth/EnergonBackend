using System;
using System.Runtime.Serialization;

namespace EnergonSoftware.Backend.Packet
{
    [Serializable]
    public class PacketException : Exception
    {
        public PacketException()
        {
        }

        public PacketException(string message) : base(message)
        {
        }

        public PacketException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PacketException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
