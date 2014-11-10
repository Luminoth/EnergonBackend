﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using log4net;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Overmind.MessageHandlers;
using EnergonSoftware.Overmind.Net;

namespace EnergonSoftware.Overmind
{
    sealed partial class Overmind : ServiceBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Overmind));

        public bool Running { get; private set; }

        private SocketListener _loginListener;
        private SessionManager _loginSessions;

        private HttpServer _diagnosticServer;

        public Overmind()
        {
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            OnStart(args);
        }

        protected async override void OnStart(string[] args)
        {
            _logger.Info("Starting overmind...");

            ListenAddressesConfigurationSection listenAddresses = (ListenAddressesConfigurationSection)ConfigurationManager.GetSection("listenAddresses");
            if(null == listenAddresses || listenAddresses.ListenAddresses.Count < 1) {
                _logger.Error("No configured listen addresses!");
// TODO: status == not running
                return;
            }

            _logger.Debug("Starting diagnostic interface...");
            _diagnosticServer = new HttpServer();
            _diagnosticServer.Start(new List<string>() { "http://localhost:9002/" });

            _logger.Debug("Opening listener sockets...");
            _loginListener = new SocketListener(new LoginSessionFactory());
            _loginListener.SocketBacklog = Convert.ToInt32(ConfigurationManager.AppSettings["socketBacklog"]);
            if(!_loginListener.CreateSockets(listenAddresses.ListenAddresses, SocketType.Stream, ProtocolType.Tcp)) {
// TODO: status == not running
                return;
            }

            _logger.Debug("Starting session manager...");
            _loginSessions = new SessionManager();
            _loginSessions.SessionTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
            _loginSessions.Start(new MessageHandlerFactory());

            _logger.Info("Running...");
            Running = true;
            await Run();
        }

        protected override void OnStop()
        {
            _logger.Info("Stopping overmind...");
            Running = false;

            _logger.Debug("Closing listener sockets...");
            _loginListener.CloseSockets();
            _loginListener = null;

            _logger.Debug("Stopping session manager...");
            _loginSessions.Stop();
            _loginSessions = null;

            _logger.Debug("Stopping diagnostic interface...");
            _diagnosticServer.Stop();
            _diagnosticServer = null;
        }

        private Task Run()
        {
            return Task.Factory.StartNew(() =>
                {
                    while(Running) {
                        try {
                            _loginListener.Poll(_loginSessions);

                            _loginSessions.PollAndRun();
                            _loginSessions.Cleanup();
                        } catch(Exception e) {
                            _logger.Fatal("Unhandled Exception!", e);
                            Running = false;
                        }
                        Thread.Sleep(0);
                    }
                }
            );
        }   
    }
}
