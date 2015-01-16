using System;
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

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await formatter.WriteAsync("ticket", SessionId).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            SessionId = await formatter.ReadStringAsync("ticket").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "SuccessMessage(SessionId=" + SessionId + ")";
        }
    }
}
