using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Configuration;
using System.Threading;

using log4net;
using log4net.Config;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Overmind.MessageHandlers;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind
{
    class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        private static volatile bool _quit = false;

        private static SocketListener _loginListener = new SocketListener(new LoginSessionFactory());
        private static SessionManager _loginSessions = new SessionManager();

        private static HttpServer _diagnosticServer = new HttpServer();

        private static void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }

        private static void OnQuit(object sender, ConsoleCancelEventArgs e)
        {
            _quit = true;

            e.Cancel = true;
        }

        private static bool Init()
        {
            Console.CancelKeyPress += OnQuit;

            int workerThreads,ioThreads;
            ThreadPool.GetMinThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMinThreads(Convert.ToInt32(ConfigurationManager.AppSettings["minWorkerThreads"]), ioThreads);

            ThreadPool.GetMaxThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMaxThreads(Convert.ToInt32(ConfigurationManager.AppSettings["maxWorkerThreads"]), ioThreads);

            _loginSessions.SessionTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);

            ListenAddressesConfigurationSection listenAddresses = (ListenAddressesConfigurationSection)ConfigurationManager.GetSection("listenAddresses");
            if(null == listenAddresses || listenAddresses.ListenAddresses.Count < 1) {
                _logger.Error("No configured listen addresses!");
                return false;
            }

            _loginListener.SocketBacklog = Convert.ToInt32(ConfigurationManager.AppSettings["socketBacklog"]);
            if(!_loginListener.CreateSockets(listenAddresses.ListenAddresses, SocketType.Stream, ProtocolType.Tcp)) {
                return false;
            }

            _loginSessions.Start(new MessageHandlerFactory());

            _diagnosticServer.Start(new List<string>() { "http://localhost:9002/" });

            return true;
        }

        private static void Cleanup()
        {
            _logger.Info("Cleaning up...");

            _loginListener.CloseSockets();
            _loginSessions.Stop();

            _diagnosticServer.Stop();
        }

        private static void Run()
        {
            _logger.Info("Running...");

            while(!_quit) {
                try {
                    _loginListener.Poll(_loginSessions);

                    _loginSessions.PollAndRun();
                    _loginSessions.Cleanup();
                } catch(Exception e) {
                    _logger.Info("Unhandled Exception!", e);
                    _quit = true;
                }

                Thread.Sleep(0);
            }
        }

        static void Main(string[] args)
        {
            ConfigureLogging();

            try {
                _logger.Info("Overmind spinning up...");
                if(!Init()) {
                    Cleanup();
                    return;
                }

                Run();

                Cleanup();
            } catch(Exception e) {
                _logger.Fatal("Unhandled exception!", e);
            }
        }
    }
}
