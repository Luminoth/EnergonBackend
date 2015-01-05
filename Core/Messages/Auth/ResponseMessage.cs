using System;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
{
    [Serializable]
    public sealed class ResponseMessage : IMessage
    {
        public const string MessageType = "response";
        public string Type { get { return MessageType; } }

        public string Response { get; set; }

        public ResponseMessage()
        {
            Response = string.Empty;
        }

        public async Task SerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            await formatter.WriteStringAsync(Response, stream).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            Response = await formatter.ReadStringAsync(stream).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "ResponseMessage(Response=" + Response + ")";
        }
    }
}
