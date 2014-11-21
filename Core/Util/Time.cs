using System;

namespace EnergonSoftware.Core.Util
{
    public static class Time
    {
        public static long CurrentTimeMs
        {
            get { return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond; }
        }
    }
}
