using System;

namespace EnergonSoftware.Backend.Messages.Parser
{
    public sealed class MessageParsedEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }
}
