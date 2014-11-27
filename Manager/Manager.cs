using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace EnergonSoftware.Manager
{
    internal sealed partial class Manager : ServiceBase
    {
        public const string ServiceId = "manager";
        public static readonly Guid UniqueId = Guid.NewGuid();

        public bool Running { get; private set; }

        public Manager()
        {
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            Task.Run(() => OnStart(args)).Wait();
        }

        protected override void OnStart(string[] args)
        {
            Running = true;

            Task.Run(() => Run()).Wait();
        }

        protected override void OnStop()
        {
            Running = false;
        }

        private void Run()
        {
            while(Running) {
                Thread.Sleep(0);
            }
        }
    }
}
