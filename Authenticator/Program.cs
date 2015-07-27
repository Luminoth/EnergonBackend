﻿using System;
using System.Configuration;
using System.Diagnostics;

using log4net;
using log4net.Config;

namespace EnergonSoftware.Authenticator
{
    internal static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        public const string EventLogSource = "Energon Software Authenticator";
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

            Console.Title = Authenticator.Instance.ServiceName;
            Console.CancelKeyPress += (sender, e) =>
                {
                    Logger.Info("Caught CancelKeyPress, stopping...");
                    Authenticator.Instance.Stop();
                    e.Cancel = true;
                };
            Authenticator.Instance.Start(Convert.ToBoolean(ConfigurationManager.AppSettings["runAsService"]), args);
        }
    }
}
