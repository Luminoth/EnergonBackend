using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Chat
{
    internal sealed partial class Chat : ServiceWrapper, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Chat));

        public const string ServiceId = "chat";
        public static readonly Guid UniqueId = Guid.NewGuid();

        public bool Running { get; private set; }

        private readonly HttpServer _diagnosticServer = new HttpServer();

        public Chat()
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

            Logger.Debug("Starting diagnostic interface...");
            _diagnosticServer.Start(new List<string>() { "http://localhost:9003/" });

            Logger.Info("Running...");
            Running = true;

            Task.Run(() => Run()).Wait();
        }

        protected override void OnStop()
        {
            Logger.Info("Stopping " + ServiceName + " with guid=" + UniqueId + "...");
            Running = false;

            Logger.Debug("Stopping diagnostic interface...");
            _diagnosticServer.Stop();
        }

        private void Run()
        {
            while(Running) {
                Thread.Sleep(0);
            }
        }
    }
}
