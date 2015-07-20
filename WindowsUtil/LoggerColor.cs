using System;
using System.Windows.Media;

using log4net.Core;

namespace EnergonSoftware.WindowsUtil
{
    /// <summary>
    /// Logger color utility methods.
    /// </summary>
    public static class LoggerColor
    {
        /// <summary>
        /// Gets the log color associated with the given level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The color</returns>
        /// <exception cref="System.ArgumentNullException">level</exception>
        public static Color LogEntryColor(Level level)
        {
            if(null == level) {
                throw new ArgumentNullException(nameof(level));
            }

            if(Level.Debug == level) {
                return Colors.DarkCyan;
            }

            if(Level.Warn == level) {
                return Colors.Green;
            }

            if(Level.Error == level) {
                return Colors.Orange;
            }

            if(Level.Fatal == level) {
                return Colors.Red;
            }

            return Colors.Black;
        }


        /// <summary>
        /// Gets the log color associated with the given log entry.
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
        /// <returns>The color</returns>
        /// <exception cref="System.ArgumentNullException">logEntry</exception>
        public static Color LogEntryColor(string logEntry)
        {
            if(null == logEntry) {
                throw new ArgumentNullException(nameof(logEntry));
            }

            if(logEntry.Contains("INFO")) {
                return LogEntryColor(Level.Info);
            }

            if(logEntry.Contains("DEBUG")) {
                return LogEntryColor(Level.Debug);
            }

            if(logEntry.Contains("WARN")) {
                return LogEntryColor(Level.Warn);
            }

            if(logEntry.Contains("ERROR")) {
                return LogEntryColor(Level.Error);
            }

            if(logEntry.Contains("FATAL")) {
                return LogEntryColor(Level.Fatal);
            }

            return Colors.Black;
        }
    }
}
