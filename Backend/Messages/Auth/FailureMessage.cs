using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
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

        public async Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Reason", Reason).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
        {
            Reason = await formatter.ReadStringAsync("Reason").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "FailureMessage(Reason=" + Reason + ")";
        }
    }
}
