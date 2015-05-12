using System;

namespace EnergonSoftware.Backend.Messages.Parser
{
    public sealed class MessageParsedEventArgs : EventArgs
    {
        public IMessage Message { get; set; }
    }
}
