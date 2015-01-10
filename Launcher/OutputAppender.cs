using EnergonSoftware.Launcher.Windows;

using log4net.Appender;
using log4net.Core;

namespace EnergonSoftware.Launcher
{
    public class OutputAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            DebugWindow.AppendOutputText(RenderLoggingEvent(loggingEvent));
        }
    }
}
