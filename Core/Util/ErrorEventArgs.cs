﻿using System;

namespace EnergonSoftware.Core.Util
{
    /// <summary>
    /// Generic error event arguments
    /// </summary>
    public sealed class ErrorEventArgs : EventArgs
    {
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
