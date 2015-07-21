using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Notification
{
    /// <summary>
    /// Service startup
    /// </summary>
    [Serializable]
    public sealed class StartupMessage : INotificationMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "startup";

        public string Type => MessageType;

        public string ServiceName { get; set; } = string.Empty;

        public string ServiceId { get; set; } = string.Empty;

        public async Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Service", ServiceName).ConfigureAwait(false);
            await formatter.WriteAsync("Id", ServiceId).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
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
