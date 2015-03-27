using System;

namespace EnergonSoftware.Core.Net.Sessions
{
    public sealed class DataReceivedEventArgs : EventArgs
    {
        public int Count { get; set; }

        public byte[] Data { get; set; }
    }
}
