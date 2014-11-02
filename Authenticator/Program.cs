﻿using System;
using System.Configuration;
using System.Threading;

using log4net;
using log4net.Config;

using EnergonSoftware.Authenticator.Net;

namespace EnergonSoftware.Authenticator
{
    class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        private static void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }

        private static bool Init()
        {
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                ServerState.Instance.Quit = true;
            };

            if(!InstanceNotifier.Instance.CreateSockets()) {
                return false;
            }

            if(!ServerState.Instance.CreateSockets()) {
                return false;
            }

            int workerThreads,ioThreads;
            ThreadPool.GetMinThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMinThreads(Convert.ToInt32(ConfigurationManager.AppSettings["minWorkerThreads"]), ioThreads);

            ThreadPool.GetMaxThreads(out workerThreads, out ioThreads);
            ThreadPool.SetMaxThreads(Convert.ToInt32(ConfigurationManager.AppSettings["maxWorkerThreads"]), ioThreads);

            return true;
        }

        private static void Cleanup()
        {
            _logger.Info("Cleaning up...");

            ServerState.Instance.CloseSockets();
            SessionManager.Instance.CloseAll();
            InstanceNotifier.Instance.CloseSockets();
        }

        private static void Run()
        {
            _logger.Info("Running...");

            while(!ServerState.Instance.Quit) {
                try {
                    InstanceNotifier.Instance.Poll();
                    ServerState.Instance.Poll();

                    SessionManager.Instance.PollAndRun();
                    SessionManager.Instance.Cleanup();
                } catch(Exception e) {
                    _logger.Info("Unhandled Exception!", e);
                    ServerState.Instance.Quit = true;
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
