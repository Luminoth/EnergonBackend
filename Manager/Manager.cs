using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Backend.MessageHandlers;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Util;

using EnergonSoftware.Manager.Configuration;
using EnergonSoftware.Manager.Diagnostics;
using EnergonSoftware.Manager.Net;

using log4net;

namespace EnergonSoftware.Manager
{
    internal sealed partial class Manager : ServiceWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Manager));

        public const string ServiceId = "manager";
        public static readonly Guid UniqueId = Guid.NewGuid();

        public static readonly Manager Instance = new Manager();

        public static ManagerServiceConfigurationSection ServiceConfigurationSection => ConfigurationManager.GetSection(ManagerServiceConfigurationSection.ConfigurationSectionName) as ManagerServiceConfigurationSection;

        public bool Running { get; private set; }

        private readonly DiagnosticsServer _diagnosticServer = new DiagnosticsServer();

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

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

            ListenAddressesConfigurationSection instanceNotifierListenAddresses = (ListenAddressesConfigurationSection)ConfigurationManager.GetSection("instanceNotifierAddresses");
            if(null == instanceNotifierListenAddresses || instanceNotifierListenAddresses.ListenAddresses.Count < 1) {
                Logger.Error("No configured instance notifier addresses!");
                Stop();
                return;
            }

            Logger.Debug("Starting diagnostic interface...");
            _diagnosticServer.Start(new List<string>() { "http://localhost:9004/" });

            Logger.Info("Starting instance notifier...");
            InstanceNotifier.Instance.StartAsync(instanceNotifierListenAddresses.ListenAddresses).Wait();

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

            Logger.Debug("Stopping instance notifier...");
            InstanceNotifier.Instance.Stop();

            Logger.Debug("Stopping diagnostic interface...");
            _diagnosticServer.Stop();
        }

        private void Run()
        {
            Running = true;
            Logger.Info("Running...");

            while(!_cancellationToken.IsCancellationRequested) {
                try {
                    Task.WhenAll(
                        InstanceNotifier.Instance.RunAsync(),
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

        private Manager()
        {
            InitializeComponent();
        }

    }
}
