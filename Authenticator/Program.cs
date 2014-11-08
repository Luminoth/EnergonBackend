using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using System.Threading;

using log4net;
using log4net.Config;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Authenticator.MessageHandlers;
using EnergonSoftware.Authenticator.Net;

namespace EnergonSoftware.Authenticator
{
    class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        private static volatile bool _quit = false;

        private static SocketListener _authListener = new SocketListener(new AuthSessionFactory());
        private static SessionManager _authSessions = new SessionManager();

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

            _authSessions.SessionTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);

            ListenAddressesConfigurationSection listenAddresses = (ListenAddressesConfigurationSection)ConfigurationManager.GetSection("listenAddresses");
            if(null == listenAddresses || listenAddresses.ListenAddresses.Count < 1) {
                _logger.Error("No configured listen addresses!");
                return false;
            }

            _authListener.SocketBacklog = Convert.ToInt32(ConfigurationManager.AppSettings["socketBacklog"]);
            if(!_authListener.CreateSockets(listenAddresses.ListenAddresses, SocketType.Stream, ProtocolType.Tcp)) {
                return false;
            }

            _authSessions.Start(new MessageHandlerFactory());

            _diagnosticServer.Start(new List<string>() { "http://localhost:9001/" });

            return true;
        }

        private static void Cleanup()
        {
            _logger.Info("Cleaning up...");

            _authListener.CloseSockets();
            _authSessions.Stop();

            _diagnosticServer.Stop();
        }

        private static void Run()
        {
            _logger.Info("Running...");

            while(!_quit) {
                try {
                    _authListener.Poll(_authSessions);

                    _authSessions.PollAndRun();
                    _authSessions.Cleanup();
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
                _logger.Info("Authenticator spinning up...");
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
