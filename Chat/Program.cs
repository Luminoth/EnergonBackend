using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;

using log4net;

namespace EnergonSoftware.Chat
{
    internal static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        internal static void Main(string[] args)
        {
            if(Convert.ToBoolean(ConfigurationManager.AppSettings["runAsService"])) {
                ServiceBase[] services = new ServiceBase[] 
                { 
                    new Chat()
                };
                ServiceBase.Run(services);
            } else {
                Chat chat = new Chat();

                Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
                {
                    Logger.Info("Caught CancelKeyPress, stopping...");
                    chat.Stop();
                    e.Cancel = true;
                };

                chat.Start(args);
                while(chat.Running) {
                    Thread.Sleep(0);
                }
            }
        }
    }
}
