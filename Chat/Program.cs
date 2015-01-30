using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using log4net;
using log4net.Config;

namespace EnergonSoftware.Chat
{
    internal static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        public const string EventLogSource = "Energon Software Chat";
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

            if(!DatabaseManager.TestDatabaseConnectionAsync().Result) {
                Logger.Fatal("Could not connect to database!");
                return;
            }

            Chat chat = new Chat();
            Console.Title = chat.ServiceName;
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                {
                    Logger.Info("Caught CancelKeyPress, stopping...");
                    chat.Stop();
                    e.Cancel = true;
                };
            chat.Start(Convert.ToBoolean(ConfigurationManager.AppSettings["runAsService"]), args);
        }
    }
}
