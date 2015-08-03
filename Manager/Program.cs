using System;
using System.Diagnostics;

using EnergonSoftware.Manager.Configuration;

using log4net;
using log4net.Config;

namespace EnergonSoftware.Manager
{
    internal static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        public const string EventLogSource = "Energon Software Manager";
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

            Console.Title = Manager.Instance.ServiceName;
            Console.CancelKeyPress += (sender, e) =>
                {
                    Logger.Info("Caught CancelKeyPress, stopping...");
                    Manager.Instance.Stop();
                    e.Cancel = true;
                };

            if(null == Manager.ServiceConfigurationSection) {
                throw new InvalidOperationException($"Missing {ManagerServiceConfigurationSection.ConfigurationSectionName} configuration section!");
            }

            Manager.Instance.Start(Manager.ServiceConfigurationSection.RunAsService, args);
        }
    }
}
