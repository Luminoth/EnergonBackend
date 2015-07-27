using System;

using EnergonSoftware.Backend.Messages;

namespace EnergonSoftware.Backend.Net.Sessions
{
    /// <summary>
    /// Emitted when a new message is received
    /// </summary>
    public sealed class MessageReceivedEventArgs
    {
        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        /// <value>
        /// The event timestamp.
        /// </value>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public Message Message { get; set; }
    }
}
