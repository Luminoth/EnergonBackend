using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using log4net;
using log4net.Config;

namespace EnergonSoftware.Overmind
{
    internal static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        public const string EventLogSource = "Energon Software Overmind";
        public static readonly EventLog ServiceEventLogger = new EventLog();

        private static void ConfigureLogging()
        {
            /*if(!EventLog.SourceExists(EventLogSource))  {         
                EventLog.CreateEventSource(EventLogSource, "Energon Software");
            }*/
            ServiceEventLogger.Source = EventLogSource;

            XmlConfigurator.Configure();
        }

        internal static void Main(string[] args)
        {
            ConfigureLogging();

            Overmind overmind = new Overmind();
            Console.Title = overmind.ServiceName;
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                {
                    Logger.Info("Caught CancelKeyPress, stopping...");
                    overmind.Stop();
                    e.Cancel = true;
                };
            overmind.Start(Convert.ToBoolean(ConfigurationManager.AppSettings["runAsService"]), args);
        }
    }
}
