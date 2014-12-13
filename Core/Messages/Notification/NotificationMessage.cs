using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Notification
{
    public interface INotificationMessage : IMessage
    {
        string ServiceName { get; set; }
        string ServiceId { get; set; }
    }
}
