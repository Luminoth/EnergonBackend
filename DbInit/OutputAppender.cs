using System;
using System.Threading.Tasks;

using EnergonSoftware.DbInit.Windows;

using log4net.Appender;
using log4net.Core;

namespace EnergonSoftware.DbInit
{
    public class OutputAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            // don't need to wait for this to finish
            try {
                Task.Run(() => MainWindow.AppendOutputTextAsync(RenderLoggingEvent(loggingEvent)));
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }
    }
}
