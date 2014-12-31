using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Notification;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Overmind.MessageHandlers;

using log4net;

namespace EnergonSoftware.Overmind.Net
{
    internal sealed class InstanceNotifier
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(InstanceNotifier));

#region Singleton
        private static readonly InstanceNotifier _instance = new InstanceNotifier();
        public static InstanceNotifier Instance { get { return _instance; } }
#endregion

        private readonly SessionManager _sessions = new SessionManager();

        public void Start(ListenAddressConfigurationElementCollection listenAddresses)
        {
            Logger.Debug("Opening instance notifier sockets...");
            foreach(ListenAddressConfigurationElement listenAddress in listenAddresses) {
                Socket listener = NetUtil.CreateMulticastListener(listenAddress.InterfaceAddress, listenAddress.Port,
                    listenAddress.MulticastGroupIPAddress, listenAddress.MulticastTTL);

                Session sender = new InstanceNotifierSession(new SocketState(listener));
                sender.ConnectMulticast(listenAddress.MulticastGroupIPAddress, listenAddress.Port, listenAddress.MulticastTTL);
                _sessions.Add(sender);
            }
        }

        public void Stop()
        {
            Logger.Debug("Disconnecting instance notifier sessions...");
            _sessions.DisconnectAll();
        }

        public async Task Run()
        {
            await Task.Run(() =>
                {
                    _sessions.PollAndRun();
                    _sessions.Cleanup();
                }
            );
        }

        public void Startup()
        {
            _sessions.BroadcastMessage(new StartupMessage()
                {
                    ServiceName = Overmind.ServiceId,
                    ServiceId = Overmind.UniqueId.ToString(),
                }
            );
        }

        public void Shutdown()
        {
            _sessions.BroadcastMessage(new ShutdownMessage()
                {
                    ServiceName = Overmind.ServiceId,
                    ServiceId = Overmind.UniqueId.ToString(),
                }
            );
        }

        private InstanceNotifier()
        {
        }
    }
}