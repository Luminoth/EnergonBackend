using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;

using log4net;

namespace EnergonSoftware.Manager
{
    internal static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        internal static void Main(string[] args)
        {
            if(Convert.ToBoolean(ConfigurationManager.AppSettings["runAsService"])) {
                ServiceBase[] services = new ServiceBase[] 
                { 
                    new Manager()
                };
                ServiceBase.Run(services);
            } else {
                Manager manager = new Manager();

                Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
                {
                    Logger.Info("Caught CancelKeyPress, stopping...");
                    manager.Stop();
                    e.Cancel = true;
                };

                manager.Start(args);
            }
        }
    }
}
