﻿using System;
using System.Diagnostics;

using EnergonSoftware.Chat.Configuration;

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

            Console.Title = Chat.Instance.ServiceName;
            Console.CancelKeyPress += (sender, e) =>
                {
                    Logger.Info("Caught CancelKeyPress, stopping...");
                    Chat.Instance.Stop();
                    e.Cancel = true;
                };

            if(null == Chat.ServiceConfigurationSection) {
                throw new InvalidOperationException($"Missing {ChatServiceConfigurationSection.ConfigurationSectionName} configuration section!");
            }

            Chat.Instance.Start(Chat.ServiceConfigurationSection.RunAsService, args);
        }
    }
}
