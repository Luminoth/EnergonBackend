﻿using System;
using System.IO;
using System.Threading.Tasks;

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

        public async Task SerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            await formatter.WriteStringAsync(ServiceName, stream).ConfigureAwait(false);
            await formatter.WriteStringAsync(ServiceId, stream).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            ServiceName = await formatter.ReadStringAsync(stream).ConfigureAwait(false);
            ServiceId = await formatter.ReadStringAsync(stream).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "StartupMessage(ServiceName=" + ServiceName + ", ServiceId=" + ServiceId + ")";
        }
    }
}
