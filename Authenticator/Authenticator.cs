﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Authenticator.MessageHandlers;
using EnergonSoftware.Authenticator.Net;
using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net;

using log4net;

namespace EnergonSoftware.Authenticator
{
    internal sealed partial class Authenticator : ServiceBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Authenticator));

        public const string ServiceId = "authenticator";
        public static readonly Guid UniqueId = Guid.NewGuid();

        public bool Running { get; private set; }

        private readonly HttpServer _diagnosticServer = new HttpServer();

        private readonly SocketListener _listener = new SocketListener(new AuthSessionFactory());
        private readonly SessionManager _sessions = new SessionManager();

        public Authenticator()
        {
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            Task.Run(() => OnStart(args)).Wait();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Starting " + ServiceName + " with guid=" + UniqueId + "...");

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
            _diagnosticServer.Start(new List<string>() { "http://localhost:9001/" });

            Logger.Info("Starting instance notifier...");
            InstanceNotifier.Instance.Start(instanceNotifierListenAddresses.ListenAddresses);

            Logger.Debug("Starting session manager...");
            _sessions.SessionTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
            _sessions.Start(new AuthMessageHandlerFactory());

            Logger.Debug("Opening listener sockets...");
            _listener.SocketBacklog = Convert.ToInt32(ConfigurationManager.AppSettings["socketBacklog"]);
            _listener.CreateSockets(listenAddresses.ListenAddresses);

            Logger.Info("Running...");
            Running = true;

            InstanceNotifier.Instance.Startup();

            Task.Run(() => Run()).Wait();
        }

        protected override void OnStop()
        {
            Logger.Info("Stopping " + ServiceName + " with guid=" + UniqueId + "...");
            Running = false;

            InstanceNotifier.Instance.Shutdown();

            Logger.Debug("Closing listener sockets...");
            _listener.CloseSockets();

            Logger.Debug("Stopping session manager...");
            _sessions.Stop();

            Logger.Debug("Stopping instance notifier...");
            InstanceNotifier.Instance.Stop();

            Logger.Debug("Stopping diagnostic interface...");
            _diagnosticServer.Stop();
        }

        private void Run()
        {
            while(Running) {
                try {
                    InstanceNotifier.Instance.Run();

                    _listener.Poll(_sessions);

                    _sessions.PollAndRun();
                    _sessions.Cleanup();
                } catch(Exception e) {
                    Logger.Fatal("Unhandled Exception!", e);
                    Stop();
                }
                Thread.Sleep(0);
            }
        }
    }
}
