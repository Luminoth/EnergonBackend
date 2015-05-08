using System;
using System.Runtime.Serialization;

namespace EnergonSoftware.Backend.MessageHandlers
{
    [Serializable]
    public class MessageHandlerException : Exception
    {
        public MessageHandlerException() : base()
        {
        }

        public MessageHandlerException(string message) : base(message)
        {
        }

        public MessageHandlerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MessageHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
