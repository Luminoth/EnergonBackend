using log4net.Appender;
using log4net.Core;

namespace EnergonSoftware.DbInit
{
    public class OutputAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            MainWindow.AppendOutputText(RenderLoggingEvent(loggingEvent));
        }
    }
}
