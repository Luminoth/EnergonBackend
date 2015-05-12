using System;
using System.Runtime.Serialization;

namespace EnergonSoftware.Backend.Messages.Formatter
{
    [Serializable]
    public class MessageFormatterException : Exception
    {
        public MessageFormatterException() : base()
        {
        }

        public MessageFormatterException(string message) : base(message)
        {
        }

        public MessageFormatterException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MessageFormatterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
