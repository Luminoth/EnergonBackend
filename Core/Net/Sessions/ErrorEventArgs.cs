﻿using System;

namespace EnergonSoftware.Core.Net.Sessions
{
    /// <summary>
    /// Emitted when a NetworkSession encounters an error
    /// </summary>
    public sealed class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the event timestamp.
        /// </summary>
        /// <value>
        /// The event timestamp.
        /// </value>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        private string _error;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string Error
        {
            get { return null != Exception && string.IsNullOrEmpty(_error) ? Exception.Message : _error; }
            set { _error = value; }
        }

        /// <summary>
        /// Gets or sets the exception associated with the error, if one exists.
        /// </summary>
        /// <value>
        /// The exception associated with the error, if one exists.
        /// </value>
        public Exception Exception { get; set; }
    }
}
