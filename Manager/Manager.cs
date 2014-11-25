using System;
using System.ServiceProcess;

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
