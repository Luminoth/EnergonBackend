using System;
using System.Windows.Media;

using log4net.Core;

namespace EnergonSoftware.WindowsUtil
{
    public static class LoggerColor
    {
        public static Color LogEntryColor(Level level)
        {
            if(Level.Debug == level) {
                return Colors.DarkCyan;
            } else if(Level.Warn == level) {
                return Colors.Green;
            } else if(Level.Error == level) {
                return Colors.Orange;
            } else if(Level.Fatal == level) {
                return Colors.Red;
            }

            return Colors.Black;
        }

        public static Color ParseColor(string logEntry)
        {
            if(null == logEntry) {
                throw new ArgumentNullException("logEntry");
            }

            if(logEntry.Contains("INFO")) {
                return LogEntryColor(Level.Info);
            } else if(logEntry.Contains("DEBUG")) {
                return LogEntryColor(Level.Debug);
            } else if(logEntry.Contains("WARN")) {
                return LogEntryColor(Level.Warn);
            } else if(logEntry.Contains("ERROR")) {
                return LogEntryColor(Level.Error);
            } else if(logEntry.Contains("FATAL")) {
                return LogEntryColor(Level.Fatal);
            }

            return Colors.Black;
        }
    }
}
