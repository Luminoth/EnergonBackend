using System;

namespace EnergonSoftware.Core.Net.Sessions
{
    public sealed class DataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        /// <value>
        /// The event timestamp.
        /// </value>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the number of bytes received.
        /// </summary>
        /// <value>
        /// The number of bytes received.
        /// </value>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the bytes received.
        /// </summary>
        /// <value>
        /// The bytes received.
        /// </value>
        public byte[] Data { get; set; }
    }
}
