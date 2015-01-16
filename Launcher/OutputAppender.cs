using System;
using System.Threading.Tasks;

using EnergonSoftware.Launcher.Windows;

using log4net.Appender;
using log4net.Core;

namespace EnergonSoftware.Launcher
{
    public class OutputAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            try {
                // don't need to wait for this to finish
                Task.Run(() => DebugWindow.AppendOutputTextAsync(RenderLoggingEvent(loggingEvent)));
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }
    }
}
