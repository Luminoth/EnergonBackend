using System;

namespace EnergonSoftware.Core.Net.Sessions
{
    /// <summary>
    /// Emitted when a NetworkSession is connected
    /// </summary>
    public sealed class ConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        /// <value>
        /// The event timestamp.
        /// </value>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}