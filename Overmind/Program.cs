﻿using System;
using System.Configuration;
using System.Diagnostics;

using EnergonSoftware.Overmind.Configuration;

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

            Console.Title = Overmind.Instance.ServiceName;
            Console.CancelKeyPress += (sender, e) =>
                {
                    Logger.Info("Caught CancelKeyPress, stopping...");
                    Overmind.Instance.Stop();
                    e.Cancel = true;
                };

            if(null == Overmind.ServiceConfigurationSection) {
                throw new InvalidOperationException($"Missing {OvermindServiceConfigurationSection.ConfigurationSectionName} configuration section!");
            }

            Overmind.Instance.Start(Overmind.ServiceConfigurationSection.RunAsService, args);
        }
    }
}
