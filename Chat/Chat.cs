using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Net.Sessions;

using EnergonSoftware.Chat.Diagnostics;
using EnergonSoftware.Chat.Net;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net.Sockets;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Chat
{
    internal sealed partial class Chat : ServiceWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Chat));

        public const string ServiceId = "chat";
        public static readonly Guid UniqueId = Guid.NewGuid();

        public bool Running { get; private set; }

        private readonly DiagnosticsServer _diagnosticServer = new DiagnosticsServer();

        private readonly TcpListener _listener = new TcpListener(new ChatSessionFactory());
        private readonly MessageSessionManager _sessions = new MessageSessionManager();

        public Chat()
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
            _diagnosticServer.Start(new List<string>() { "http://localhost:9003/" });

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
            _listener.CloseSockets();

            Logger.Debug("Disconnecting sessions...");
            _sessions.DisconnectAllAsync().Wait();

            Logger.Debug("Stopping instance notifier...");
            InstanceNotifier.Instance.Stop();

            Logger.Debug("Stopping diagnostic interface...");
            _diagnosticServer.Stop();

            EventLogger.Instance.ShutdownEventAsync().Wait();
        }

        private async Task PollAndReadAllAsync()
        {
            await _listener.PollAsync(_sessions).ConfigureAwait(false);

            await _sessions.PollAndReadAllAsync(100).ConfigureAwait(false);
            _sessions.Cleanup();
        }

        private void Run()
        {
            while(Running) {
                try {
                    Task.WhenAll(InstanceNotifier.Instance.RunAsync(), PollAndReadAllAsync()).Wait();
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
