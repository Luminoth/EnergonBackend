using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Notification
{
    /// <summary>
    /// Service shutdown
    /// </summary>
    [Serializable]
    public sealed class ShutdownMessage : NotificationMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "shutdown";

        public override string Type => MessageType;

        public override string ServiceName { get; set; } = string.Empty;

        public override string ServiceId { get; set; } = string.Empty;

        public async override Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Service", ServiceName).ConfigureAwait(false);
            await formatter.WriteAsync("Id", ServiceId).ConfigureAwait(false);
        }

        public async override Task DeserializeAsync(IFormatter formatter)
        {
            ServiceName = await formatter.ReadStringAsync("Service").ConfigureAwait(false);
            ServiceId = await formatter.ReadStringAsync("Id").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return $"ShutdownMessage(ServiceName={ServiceName}, ServiceId={ServiceId})";
        }
    }
}
