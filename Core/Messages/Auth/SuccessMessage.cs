using System;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
{
    [Serializable]
    public sealed class SuccessMessage : IMessage
    {
        public const string MessageType = "success";
        public string Type { get { return MessageType; } }

        public string SessionId { get; set; }

        public SuccessMessage()
        {
            SessionId = string.Empty;
        }

        public async Task SerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            await formatter.WriteStringAsync(SessionId, stream).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            SessionId = await formatter.ReadStringAsync(stream).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "SuccessMessage(SessionId=" + SessionId + ")";
        }
    }
}
