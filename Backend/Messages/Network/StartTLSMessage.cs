using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Network
{
    /// <summary>
    /// Requests TLS authentication
    /// </summary>
    [Serializable]
    // ReSharper disable once InconsistentNaming
    public sealed class StartTLSMessage : IMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "starttls";

        public string Type { get { return MessageType; } }

        public async Task SerializeAsync(IFormatter formatter)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "StartTLSMessage()";
        }
    }
}
