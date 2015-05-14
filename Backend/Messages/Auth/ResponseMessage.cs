using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

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

        public async Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Response", Response).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
        {
            Response = await formatter.ReadStringAsync("Response").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "ResponseMessage(Response=" + Response + ")";
        }
    }
}
