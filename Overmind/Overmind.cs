﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Overmind.MessageHandlers;
using EnergonSoftware.Overmind.Net;

using log4net;

namespace EnergonSoftware.Overmind
{
    internal sealed partial class Overmind : ServiceBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Overmind));

        public bool Running { get; private set; }

        private HttpServer _diagnosticServer = new HttpServer();

        private SocketListener _loginListener = new SocketListener(new LoginSessionFactory());
        private SessionManager _loginSessions = new SessionManager();

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
            Logger.Info("Starting overmind...");

            ListenAddressesConfigurationSection listenAddresses = (ListenAddressesConfigurationSection)ConfigurationManager.GetSection("listenAddresses");
            if(null == listenAddresses || listenAddresses.ListenAddresses.Count < 1) {
                Logger.Error("No configured listen addresses!");
                Stop();
                return;
            }

            ListenAddressesConfigurationSection instanceNotifierListenAddresses = (ListenAddressesConfigurationSection)ConfigurationManager.GetSection("instanceNotifierAddresses");
            if(null == instanceNotifierListenAddresses || instanceNotifierListenAddresses.ListenAddresses.Count < 1) {
                Logger.Error("No configured instance notifier addresses!");
                Stop();
                return;
            }

            Logger.Debug("Starting diagnostic interface...");
            _diagnosticServer.Start(new List<string>() { "http://localhost:9002/" });

            Logger.Info("Starting instance notifier...");
            InstanceNotifier.Instance.Start(instanceNotifierListenAddresses.ListenAddresses);

            Logger.Debug("Opening listener sockets...");
            _loginListener.SocketBacklog = Convert.ToInt32(ConfigurationManager.AppSettings["socketBacklog"]);
            _loginListener.CreateSockets(listenAddresses.ListenAddresses);

            Logger.Debug("Starting session manager...");
            _loginSessions.SessionTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
            _loginSessions.Start(new MessageHandlerFactory());

            Logger.Info("Running...");
            Running = true;
            await Run();
        }

        protected override void OnStop()
        {
            Logger.Info("Stopping overmind...");
            Running = false;

            Logger.Debug("Closing listener sockets...");
            _loginListener.CloseSockets();

            Logger.Debug("Stopping session manager...");
            _loginSessions.Stop();

            Logger.Debug("Stopping instance notifier...");
            InstanceNotifier.Instance.Stop();

            Logger.Debug("Stopping diagnostic interface...");
            _diagnosticServer.Stop();
        }

        private Task Run()
        {
            return Task.Factory.StartNew(() =>
                {
                    while(Running) {
                        try {
                            InstanceNotifier.Instance.Run();

                            _loginListener.Poll(_loginSessions);

                            _loginSessions.PollAndRun();
                            _loginSessions.Cleanup();
                        } catch(Exception e) {
                            Logger.Fatal("Unhandled Exception!", e);
                            Stop();
                        }
                        Thread.Sleep(0);
                    }
                }
            );
        }   
    }
}