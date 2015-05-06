using System;

using EnergonSoftware.Core.Messages;

namespace EnergonSoftware.Core.MessageHandlers
{
    public sealed class HandleMessageEventArgs : EventArgs
    {
        public IMessage Message { get; set; }
    }
}
