using System;

namespace EnergonSoftware.Core.Net
{
    public sealed class DisconnectEventArgs : EventArgs
    {
        public string Reason { get; set; }
    }
}
