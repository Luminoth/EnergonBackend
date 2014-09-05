using System;
using System.Runtime.Serialization;

namespace EnergonSoftware.Core.Messages
{
    [Serializable]
    class MessageException : Exception
    {
        public MessageException() : base()
        {
        }

        public MessageException(string message) : base(message)
        {
        }

        public MessageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MessageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
