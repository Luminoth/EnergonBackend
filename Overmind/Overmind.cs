using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Net.Sessions;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net.Sockets;
using EnergonSoftware.Core.Util;

using EnergonSoftware.Overmind.Diagnostics;
using EnergonSoftware.Overmind.Net;

using log4net;

namespace EnergonSoftware.Overmind
{
    internal sealed partial class Overmind : ServiceWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Overmind));

        public const string ServiceId = "overmind";
        public static readonly Guid UniqueId = Guid.NewGuid();

        public bool Running { get; private set; }

        private readonly DiagnosticsServer _diagnosticServer = new DiagnosticsServer();

        private readonly TcpListener _listener = new TcpListener(new OvermindSessionFactory());
        private readonly MessageNetworkSessionManager _sessions = new MessageNetworkSessionManager();

        public Overmind()
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
            _diagnosticServer.Start(new List<string>() { "http://localhost:9002/" });

            Logger.Info("Starting instance notifier...");
            InstanceNotifier.Instance.StartAsync(instanceNotifierListenAddresses.ListenAddresses).Wait();

            Logger.Debug("Opening listener sockets...");
            _listener.MaxConnections = listenAddresses.MaxConnections;
            _listener.SocketBacklog = listenAddresses.Backlog;
            _listener.CreateSockets(listenAddresses.ListenAddresses);

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
            _listener.CloseSocketsAsync().Wait();

            Logger.Debug("Disconnecting sessions...");
            _sessions.DisconnectAllAsync().Wait();

            Logger.Debug("Stopping instance notifier...");
            InstanceNotifier.Instance.Stop();

            Logger.Debug("Stopping diagnostic interface...");
            _diagnosticServer.Stop();

            EventLogger.Instance.ShutdownEventAsync().Wait();
        }

        private async Task PollAndReceiveAllAsync()
        {
            await _listener.PollAsync(_sessions).ConfigureAwait(false);

            await _sessions.PollAndReceiveAllAsync(100).ConfigureAwait(false);
            await _sessions.CleanupAsync().ConfigureAwait(false);
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
