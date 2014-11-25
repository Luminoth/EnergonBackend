using System;
using System.ServiceProcess;

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
            OnStart(args);
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
