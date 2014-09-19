using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
{
    public sealed class ResponseMessage : IMessage
    {
        public string Type { get { return "response"; } }

        public string Response = "";

        public ResponseMessage()
        {
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteString(Response, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            Response = formatter.ReadString(stream);
        }
    }
}
