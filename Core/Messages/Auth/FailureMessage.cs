using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
{
    [Serializable]
    public sealed class FailureMessage : IMessage
    {
        public const string MessageType = "failure";
        public string Type { get { return MessageType; } }

        public string Reason { get; set; }

        public FailureMessage()
        {
            Reason = string.Empty;
        }

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await formatter.WriteAsync("reason", Reason).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            Reason = await formatter.ReadStringAsync("reason").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "FailureMessage(Reason=" + Reason + ")";
        }
    }
}
