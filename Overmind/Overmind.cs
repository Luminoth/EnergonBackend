using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;
using EnergonSoftware.Backend.Net.Sessions;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net.Sockets;
using EnergonSoftware.Core.Util;

using EnergonSoftware.Overmind.Configuration;
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

        public static readonly Overmind Instance = new Overmind();

        public static OvermindServiceConfigurationSection ServiceConfigurationSection => ConfigurationManager.GetSection(OvermindServiceConfigurationSection.ConfigurationSectionName) as OvermindServiceConfigurationSection;

        public bool Running { get; private set; }

        private readonly DiagnosticsServer _diagnosticServer = new DiagnosticsServer();

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

#region Network Sessions
        private readonly TcpListener _listener = new TcpListener();

        private readonly MessageNetworkSessionManager _sessionManager = new MessageNetworkSessionManager();
#endregion

#region Message Processing
        public MessageProcessor MessageProcessor  { get; } = new MessageProcessor();
#endregion

#region Dispose
        protected override void Dispose(bool disposing)
        {
            if(disposing) {
                components?.Dispose();
                _diagnosticServer.Dispose();
                _cancellationToken.Dispose();
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

            _sessionManager.SessionFactory = new OvermindSessionFactory();
            _sessionManager.MaxSessions = listenAddresses.MaxConnections;

            Logger.Debug("Opening listener sockets...");
            _listener.SocketBacklog = listenAddresses.Backlog;
            _listener.NewConnectionEvent += _sessionManager.NewConnectionEventHandlerAsync;
            _listener.CreateSocketsAsync(listenAddresses.ListenAddresses).Wait();

            EventLogger.Instance.StartupEventAsync().Wait();
            InstanceNotifier.Instance.StartupAsync().Wait();

            Run();
        }

        protected override void OnStop()
        {
            Logger.Info($"Stopping {ServiceName} with guid={UniqueId}...");

            _cancellationToken.Cancel();
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
            Running = true;
            Logger.Info("Running...");

            while(!_cancellationToken.IsCancellationRequested) {
                try {
                    Task.WhenAll(
                        InstanceNotifier.Instance.RunAsync(),
                        PollAndReceiveAllAsync(),
                        MessageProcessor.RunAsync()
                    ).Wait();
                } catch(Exception e) {
                    Logger.Fatal("Unhandled Exception!", e);
                    Stop();
                }

                Thread.Sleep(0);
            }

            Running = false;
            Cleanup();
        }

        private Overmind()
        {
            InitializeComponent();
        }
    }
}
