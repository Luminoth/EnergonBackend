using System;

using EnergonSoftware.Backend.Messages;

namespace EnergonSoftware.Backend.MessageHandlers
{
    /// <summary>
    /// Emitted when a new message is ready to be handled
    /// </summary>
    public sealed class HandleMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public Message Message { get; set; }
    }
}
