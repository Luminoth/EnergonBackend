using System;

namespace EnergonSoftware.Core.Net.Sessions
{
    public sealed class ConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        /// <value>
        /// The event timestamp.
        /// </value>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedEventArgs"/> class.
        /// </summary>
        public ConnectedEventArgs()
        {
            Timestamp = DateTime.Now;
        }
    }
}