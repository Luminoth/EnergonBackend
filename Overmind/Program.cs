using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

using log4net;
using log4net.Config;

namespace EnergonSoftware.Overmind
{
    class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        public const string EVENT_LOG_SOURCE = "Energon Software Overmind";
        public static EventLog ServiceEventLogger = new EventLog();

        private static void ConfigureLogging()
        {
            if(!EventLog.SourceExists(EVENT_LOG_SOURCE))  {         
                EventLog.CreateEventSource(EVENT_LOG_SOURCE, "Energon Software");
            }
            ServiceEventLogger.Source = EVENT_LOG_SOURCE;

            XmlConfigurator.Configure();
        }

        private static void ConfigureThreading()
        {
            int workerThreads,ioThreads;
            ThreadPool.GetMinThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMinThreads(Convert.ToInt32(ConfigurationManager.AppSettings["minWorkerThreads"]), ioThreads);

            ThreadPool.GetMaxThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMaxThreads(Convert.ToInt32(ConfigurationManager.AppSettings["maxWorkerThreads"]), ioThreads);
        }

        static void Main(string[] args)
        {
            ConfigureLogging();
            ConfigureThreading();

            if(Convert.ToBoolean(ConfigurationManager.AppSettings["runAsService"])) {
                ServiceBase[] services = new ServiceBase[] 
                { 
                    new Overmind()
                };
                ServiceBase.Run(services);
            } else {
                Overmind overmind = new Overmind();

                Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
                {
                    overmind.Stop();
                    e.Cancel = true;
                };

                overmind.Start(args);
                while(overmind.Running) {
                    Thread.Sleep(0);
                }
            }
        }
    }
}
