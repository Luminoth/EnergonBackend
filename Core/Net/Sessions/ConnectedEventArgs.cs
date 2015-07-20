﻿using System;

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
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}