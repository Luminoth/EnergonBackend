using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Network
{
    [Serializable]
    public sealed class StartTLSMessage : IMessage
    {
        public const string MessageType = "starttls";
        public string Type { get { return MessageType; } }

        public StartTLSMessage()
        {
        }

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "StartTLSMessage()";
        }
    }
}
