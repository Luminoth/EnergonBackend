using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Notifications
{
    public interface INotificationMessage : IMessage
    {
        string ServiceName { get; set; }
        string ServiceId { get; set; }
    }
}
