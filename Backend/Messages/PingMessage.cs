using System;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages.Formatter;

namespace EnergonSoftware.Backend.Messages
{
    [Serializable]
    public sealed class PingMessage : IMessage
    {
        public const string MessageType = "ping";
        public string Type { get { return MessageType; } }

        public PingMessage()
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
            return "PingMessage()";
        }
    }
}
