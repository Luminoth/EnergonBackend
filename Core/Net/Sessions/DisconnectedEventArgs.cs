using System;

namespace EnergonSoftware.Core.Net.Sessions
{
    public sealed class DisconnectedEventArgs : EventArgs
    {
        public string Reason { get; set; }
    }
}
