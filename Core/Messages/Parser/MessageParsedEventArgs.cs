using System;

namespace EnergonSoftware.Core.Messages.Parser
{
    public sealed class MessageParsedEventArgs : EventArgs
    {
        public IMessage Message { get; set; }
    }
}
