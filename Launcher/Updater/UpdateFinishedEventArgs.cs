using System;

namespace EnergonSoftware.Launcher.Updater
{
    internal sealed class UpdateFinishedEventArgs : EventArgs
    {
        public bool Success { get; set; }
    }
}
