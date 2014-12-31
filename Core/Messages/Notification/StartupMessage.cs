using System;
using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Notification
{
    [Serializable]
    public sealed class StartupMessage : INotificationMessage
    {
        public const string MessageType = "startup";
        public string Type { get { return MessageType; } }

        public string ServiceName { get; set; }
        public string ServiceId { get; set; }

        public StartupMessage() : base()
        {
            ServiceName = string.Empty;
            ServiceId = string.Empty;
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteString(ServiceName, stream);
            formatter.WriteString(ServiceId, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            ServiceName = formatter.ReadString(stream);
            ServiceId = formatter.ReadString(stream);
        }

        public override string ToString()
        {
            return "StartupMessage(ServiceName=" + ServiceName + ", ServiceId=" + ServiceId + ")";
        }
    }
}
