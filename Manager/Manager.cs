using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;
using EnergonSoftware.Manager.Net;

using log4net;

namespace EnergonSoftware.Manager
{
    internal sealed partial class Manager : ServiceWrapper, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Manager));

        public const string ServiceId = "manager";
        public static readonly Guid UniqueId = Guid.NewGuid();

        public bool Running { get; private set; }

        private readonly HttpServer _diagnosticServer = new HttpServer();

        public Manager()
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

            ListenAddressesConfigurationSection instanceNotifierListenAddresses = (ListenAddressesConfigurationSection)ConfigurationManager.GetSection("instanceNotifierAddresses");
            if(null == instanceNotifierListenAddresses || instanceNotifierListenAddresses.ListenAddresses.Count < 1) {
                Logger.Error("No configured instance notifier addresses!");
                Stop();
                return;
            }

            Logger.Debug("Starting diagnostic interface...");
            _diagnosticServer.Start(new List<string>() { "http://localhost:9004/" });

            Logger.Info("Starting instance notifier...");
            InstanceNotifier.Instance.Start(instanceNotifierListenAddresses.ListenAddresses);

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

            Logger.Debug("Stopping instance notifier...");
            InstanceNotifier.Instance.Stop();

            Logger.Debug("Stopping diagnostic interface...");
            _diagnosticServer.Stop();
        }

        private void Run()
        {
            while(Running) {
                List<Task> tasks = new List<Task>();
                try {
                    tasks.Add(Task.Run(() => InstanceNotifier.Instance.Run()));

                    Task.WhenAll(tasks).Wait();
                } catch(Exception e) {
                    Logger.Fatal("Unhandled Exception!", e);
                    Stop();
                }
                Thread.Sleep(0);
            }
        }
    }
}
