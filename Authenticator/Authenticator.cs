﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using log4net;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Authenticator.MessageHandlers;
using EnergonSoftware.Authenticator.Net;

namespace EnergonSoftware.Authenticator
{
    sealed partial class Authenticator : ServiceBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Authenticator));

        public bool Running { get; private set; }

        private HttpServer _diagnosticServer = new HttpServer();

        private SocketListener _authListener = new SocketListener(new AuthSessionFactory());
        private SessionManager _authSessions = new SessionManager();

        public Authenticator()
        {
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            OnStart(args);
        }

        protected async override void OnStart(string[] args)
        {
            _logger.Info("Starting authenticator...");

            ListenAddressesConfigurationSection listenAddresses = (ListenAddressesConfigurationSection)ConfigurationManager.GetSection("listenAddresses");
            if(null == listenAddresses || listenAddresses.ListenAddresses.Count < 1) {
                _logger.Error("No configured listen addresses!");
                Stop();
                return;
            }

            ListenAddressesConfigurationSection instanceNotifierListenAddresses = (ListenAddressesConfigurationSection)ConfigurationManager.GetSection("instanceNotifierAddresses");
            if(null == instanceNotifierListenAddresses || instanceNotifierListenAddresses.ListenAddresses.Count < 1) {
                _logger.Error("No configured instance notifier addresses!");
                Stop();
                return;
            }

            _logger.Debug("Starting diagnostic interface...");
            _diagnosticServer.Start(new List<string>() { "http://localhost:9001/" });

            _logger.Info("Creating instance notifier multicast socket...");
            InstanceNotifier.Instance.CreateSockets(instanceNotifierListenAddresses.ListenAddresses);

            _logger.Debug("Opening listener sockets...");
            _authListener.SocketBacklog = Convert.ToInt32(ConfigurationManager.AppSettings["socketBacklog"]);
            _authListener.CreateSockets(listenAddresses.ListenAddresses, SocketType.Stream, ProtocolType.Tcp);

            _logger.Debug("Starting session manager...");
            _authSessions.SessionTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
            _authSessions.Start(new MessageHandlerFactory());

            _logger.Info("Running...");
            Running = true;
            await Run();
        }

        protected override void OnStop()
        {
            _logger.Info("Stopping authenticator...");
            Running = false;

            _logger.Debug("Closing listener sockets...");
            _authListener.CloseSockets();

            _logger.Debug("Stopping session manager...");
            _authSessions.Stop();

            _logger.Debug("Stopping diagnostic interface...");
            _diagnosticServer.Stop();
        }

        private Task Run()
        {
            return Task.Factory.StartNew(() =>
                {
                    while(Running) {
                        try {
                            InstanceNotifier.Instance.Run();

                            _authListener.Poll(_authSessions);

                            _authSessions.PollAndRun();
                            _authSessions.Cleanup();
                        } catch(Exception e) {
                            _logger.Fatal("Unhandled Exception!", e);
                            Stop();
                        }
                        Thread.Sleep(0);
                    }
                }
            );
        }
    }
}
