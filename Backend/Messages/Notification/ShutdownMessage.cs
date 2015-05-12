using System;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages.Formatter;

namespace EnergonSoftware.Backend.Messages.Notification
{
    [Serializable]
    public sealed class ShutdownMessage : INotificationMessage
    {
        public const string MessageType = "shutdown";
        public string Type { get { return MessageType; } }

        public string ServiceName { get; set; }
        public string ServiceId { get; set; }

        public ShutdownMessage() : base()
        {
            ServiceName = string.Empty;
            ServiceId = string.Empty;
        }

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await formatter.WriteAsync("service", ServiceName).ConfigureAwait(false);
            await formatter.WriteAsync("id", ServiceId).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            ServiceName = await formatter.ReadStringAsync("service").ConfigureAwait(false);
            ServiceId = await formatter.ReadStringAsync("id").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "ShutdownMessage(ServiceName=" + ServiceName + ", ServiceId=" + ServiceId + ")";
        }
    }
}
