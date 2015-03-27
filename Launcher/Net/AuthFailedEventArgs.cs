using System;

namespace EnergonSoftware.Launcher.Net
{
    public sealed class AuthFailedEventArgs : EventArgs
    {
        public string Reason { get; set; }
    }
}