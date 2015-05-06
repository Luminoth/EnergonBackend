using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Backend.Messages.Auth
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

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await formatter.WriteAsync("response", Response).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            Response = await formatter.ReadStringAsync("response").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "ResponseMessage(Response=" + Response + ")";
        }
    }
}
