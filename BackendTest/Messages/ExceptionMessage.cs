using System;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;

namespace EnergonSoftware.Backend.Test.Messages
{
    [Serializable]
    public sealed class ExceptionMessage : IMessage
    {
        public const string MessageType = "exception";
        public string Type { get { return MessageType; } }

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}
