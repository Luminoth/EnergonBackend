using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Authenticator.Diagnostics;
using EnergonSoftware.Authenticator.Net;

using EnergonSoftware.Backend.Net.Sessions;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net.Sockets;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Authenticator
{
    internal sealed partial class Authenticator : ServiceWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Authenticator));

        public const string ServiceId = "authenticator";
        public static readonly Guid UniqueId = Guid.NewGuid();

        public bool Running { get; private set; }

        private readonly DiagnosticsServer _diagnosticServer = new DiagnosticsServer();

        private readonly TcpListener _listener = new TcpListener();
        private readonly MessageSessionManager _sessionManager = new MessageSessionManager();

        public Authenticator()
        {
            InitializeComponent();
        }

#region Dispose
        protected override void Dispose(bool disposing)
        {
            if(disposing) {
                components?.Dispose();
                _diagnosticServer.Dispose();
            }

            base.Dispose(disposing);
        }
#endregion

        protected override void OnStart(string[] args)
        {
            Logger.Info($"Starting {ServiceName} with guid={UniqueId}...");

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
            InstanceNotifier.Instance.StartAsync(instanceNotifierListenAddresses.ListenAddresses).Wait();

            _sessionManager.SessionFactory = new AuthSessionFactory();
            _sessionManager.MaxSessions = listenAddresses.MaxConnections;

            Logger.Debug("Opening listener sockets...");
            _listener.SocketBacklog = listenAddresses.Backlog;
            _listener.NewConnectionEvent += _sessionManager.NewConnectionEventHandlerAsync;
            _listener.CreateSocketsAsync(listenAddresses.ListenAddresses).Wait();

            Logger.Info("Running...");
            Running = true;

            EventLogger.Instance.StartupEventAsync().Wait();
            InstanceNotifier.Instance.StartupAsync().Wait();

            Run();
        }

        protected override void OnStop()
        {
            Logger.Info($"Stopping {ServiceName} with guid={UniqueId}...");
            Running = false;
        }

        private void Cleanup()
        {
            Logger.Info("Cleaning up...");
            InstanceNotifier.Instance.ShutdownAsync().Wait();

            Logger.Debug("Closing listener sockets...");
            _listener.NewConnectionEvent -= _sessionManager.NewConnectionEventHandlerAsync;
            _listener.CloseSocketsAsync().Wait();

            Logger.Debug("Disconnecting sessions...");
            _sessionManager.DisconnectAllAsync().Wait();

            Logger.Debug("Stopping instance notifier...");
            InstanceNotifier.Instance.Stop();

            Logger.Debug("Stopping diagnostic interface...");
            _diagnosticServer.Stop();

            EventLogger.Instance.ShutdownEventAsync().Wait();
        }

        private async Task PollAndReceiveAllAsync()
        {
            await _listener.PollAsync(100).ConfigureAwait(false);

            await _sessionManager.PollAndReceiveAllAsync(100).ConfigureAwait(false);
            await _sessionManager.CleanupAsync().ConfigureAwait(false);
        }

        private void Run()
        {
            while(Running) {
                try {
                    Task.WhenAll(InstanceNotifier.Instance.RunAsync(), PollAndReceiveAllAsync()).Wait();
                } catch(Exception e) {
                    Logger.Fatal("Unhandled Exception!", e);
                    Stop();
                }

                Thread.Sleep(0);
            }

            Cleanup();
        }
    }
}
