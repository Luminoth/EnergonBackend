using System;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Test.Messages
{
    [Serializable]
    public sealed class ExceptionMessage : IMessage
    {
        public const string MessageType = "exception";
        public string Type { get { return MessageType; } }

        public ExceptionMessage()
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
    }
}
