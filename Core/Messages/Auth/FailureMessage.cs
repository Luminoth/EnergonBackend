using System;
using System.IO;
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

        public async Task SerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            await formatter.WriteStringAsync(Reason, stream).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            Reason = await formatter.ReadStringAsync(stream).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "FailureMessage(Reason=" + Reason + ")";
        }
    }
}
