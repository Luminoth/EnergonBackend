using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Overmind.MessageHandlers;
using EnergonSoftware.Overmind.Net;

using log4net;

namespace EnergonSoftware.Overmind
{
    internal sealed partial class Overmind : ServiceWrapper, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Overmind));

        public const string ServiceId = "overmind";
        public static readonly Guid UniqueId = Guid.NewGuid();

        public bool Running { get; private set; }

        private readonly HttpServer _diagnosticServer = new HttpServer();

        private readonly SocketListener _loginListener = new SocketListener(new LoginSessionFactory());
        private readonly SessionManager _loginSessions = new SessionManager();

        public Overmind()
        {
            InitializeComponent();
        }

#region Dispose
        protected override void Dispose(bool disposing)
        {
            if(disposing) {
                if(components != null) {
                    components.Dispose();
                }

                _diagnosticServer.Dispose();
            }
            base.Dispose(disposing);
        }
#endregion

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
            _diagnosticServer.Start(new List<string>() { "http://localhost:9002/" });

            Logger.Info("Starting instance notifier...");
            InstanceNotifier.Instance.Start(instanceNotifierListenAddresses.ListenAddresses);

            Logger.Debug("Opening listener sockets...");
            _loginListener.SocketBacklog = Convert.ToInt32(ConfigurationManager.AppSettings["socketBacklog"]);
            _loginListener.CreateSockets(listenAddresses.ListenAddresses);

            Logger.Info("Running...");
            Running = true;

            InstanceNotifier.Instance.Startup();

            Run();
        }

        protected override void OnStop()
        {
            Logger.Info("Stopping " + ServiceName + " with guid=" + UniqueId + "...");
            Running = false;

            InstanceNotifier.Instance.Shutdown();

            Logger.Debug("Closing listener sockets...");
            _loginListener.CloseSockets();

            Logger.Debug("Disconnecting sessions...");
            _loginSessions.DisconnectAll();

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
    }
}
