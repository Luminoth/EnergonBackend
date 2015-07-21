using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages
{
    /// <summary>
    /// Ping
    /// </summary>
    [Serializable]
    public sealed class PingMessage : Message
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "ping";

        public override string Type => MessageType;

        public async override Task SerializeAsync(IFormatter formatter)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public async override Task DeserializeAsync(IFormatter formatter)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "PingMessage()";
        }
    }
}
