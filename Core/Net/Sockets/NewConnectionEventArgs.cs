﻿using System;
using System.Net.Sockets;

namespace EnergonSoftware.Core.Net.Sockets
{
    /// <summary>
    /// Emitted when a new connection arrives at a listener
    /// </summary>
    public sealed class NewConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        /// <value>
        /// The event timestamp.
        /// </value>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the newly connected socket.
        /// </summary>
        /// <value>
        /// The newly connected socket.
        /// </value>
        public Socket Socket { get; set; }
    }
}