using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages
{
    public interface NotificationMessage : IMessage
    {
        string ServiceName { get; set; }
        string ServiceId { get; set; }
    }
}
