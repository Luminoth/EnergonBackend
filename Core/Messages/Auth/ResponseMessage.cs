using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
{
    public sealed class ResponseMessage : IMessage
    {
        public const string MessageType = "response";
        public string Type { get { return MessageType; } }

        public string Response { get; set; }

        public ResponseMessage()
        {
            Response = string.Empty;
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteString(Response, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            Response = formatter.ReadString(stream);
        }

        public override string ToString()
        {
            return "ResponseMessage(Response=" + Response + ")";
        }
    }
}
