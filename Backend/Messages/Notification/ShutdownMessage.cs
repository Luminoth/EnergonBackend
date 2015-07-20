﻿using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Notification
{
    /// <summary>
    /// Service shutdown
    /// </summary>
    [Serializable]
    public sealed class ShutdownMessage : INotificationMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "shutdown";

        public string Type { get { return MessageType; } }

        public string ServiceName { get; set; }

        public string ServiceId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShutdownMessage"/> class.
        /// </summary>
        public ShutdownMessage()
        {
            ServiceName = string.Empty;
            ServiceId = string.Empty;
        }

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
            return "ShutdownMessage(ServiceName=" + ServiceName + ", ServiceId=" + ServiceId + ")";
        }
    }
}
