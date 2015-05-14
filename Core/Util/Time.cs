using System;

namespace EnergonSoftware.Core.Util
{
    public static class Time
    {
        /// <summary>
        /// Gets the current time in milliseconds.
        /// </summary>
        /// <value>
        /// The current time in milliseconds.
        /// </value>
        public static long CurrentTimeMs
        {
            get { return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond; }
        }
    }
}
