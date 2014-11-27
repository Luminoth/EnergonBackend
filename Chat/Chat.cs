using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace EnergonSoftware.Chat
{
    internal sealed partial class Chat : ServiceBase
    {
        public const string ServiceId = "chat";
        public static readonly Guid UniqueId = Guid.NewGuid();

        public bool Running { get; private set; }

        public Chat()
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
