using System;
using System.Net.Sockets;

namespace EnergonSoftware.Core.Net.Sockets
{
    public sealed class NewConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        /// <value>
        /// The event timestamp.
        /// </value>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the newly connected socket.
        /// </summary>
        /// <value>
        /// The newly connected socket.
        /// </value>
        public Socket Socket { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewConnectionEventArgs" /> class.
        /// </summary>
        public NewConnectionEventArgs()
        {
            Timestamp = DateTime.Now;
        }
    }
}