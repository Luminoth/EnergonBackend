using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Notification
{
    /// <summary>
    /// Service startup
    /// </summary>
    [Serializable]
    public sealed class StartupMessage : NotificationMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "startup";

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
            return $"StartupMessage(ServiceName={ServiceName}, ServiceId={ServiceId})";
        }
    }
}
