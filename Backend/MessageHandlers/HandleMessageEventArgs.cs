using System;

using EnergonSoftware.Backend.Messages;

namespace EnergonSoftware.Backend.MessageHandlers
{
    public sealed class HandleMessageEventArgs : EventArgs
    {
        public IMessage Message { get; set; }
    }
}
