using System;

namespace EnergonSoftware.Core.Net.Sessions
{
    /// <summary>
    /// Emitted when a NetworkSession disconnects
    /// </summary>
    public sealed class DisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        /// <value>
        /// The event timestamp.
        /// </value>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the reason for the disconnect.
        /// This may be null.
        /// </summary>
        /// <value>
        /// The reason for the disconnect.
        /// </value>
        public string Reason { get; set; }
    }
}
