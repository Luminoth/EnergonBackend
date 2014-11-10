using System;
using System.Collections.Generic;
using System.Configuration;
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

        private SocketListener _authListener;
        private SessionManager _authSessions;

        private HttpServer _diagnosticServer;

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
// TODO: status == not running
                return;
            }

            _logger.Debug("Starting diagnostic interface...");
            _diagnosticServer = new HttpServer();
            _diagnosticServer.Start(new List<string>() { "http://localhost:9001/" });

            _logger.Debug("Opening listener sockets...");
            _authListener = new SocketListener(new AuthSessionFactory());
            _authListener.SocketBacklog = Convert.ToInt32(ConfigurationManager.AppSettings["socketBacklog"]);
            if(!_authListener.CreateSockets(listenAddresses.ListenAddresses, SocketType.Stream, ProtocolType.Tcp)) {
// TODO: status == not running
                return;
            }

            _logger.Debug("Starting session manager...");
            _authSessions = new SessionManager();
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
            _authListener = null;

            _logger.Debug("Stopping session manager...");
            _authSessions.Stop();
            _authSessions = null;

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
                            _authListener.Poll(_authSessions);

                            _authSessions.PollAndRun();
                            _authSessions.Cleanup();
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
