// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// Useful DateTime extensions
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Gets a DateTime from the given ticks, represented in milliseconds.
        /// </summary>
        /// <param name="ticksMs">The ticks in milliseconds.</param>
        /// <returns>A DateTime from the given ticks, represented in milliseconds</returns>
        public static DateTime DateTimeFromTicksMs(long ticksMs)
        {
            return new DateTime(ticksMs * TimeSpan.TicksPerMillisecond);
        }

        /// <summary>
        /// Gets the DateTime ticks in milliseconds.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>The DateTime ticks in milliseconds</returns>
        public static long GetTicksMs(this DateTime dateTime)
        {
            return dateTime.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
