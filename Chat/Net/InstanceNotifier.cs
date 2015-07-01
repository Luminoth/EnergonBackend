using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages.Notification;
using EnergonSoftware.Backend.Net.Sessions;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Net.Sessions;

using log4net;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class InstanceNotifier
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(InstanceNotifier));

        public static readonly InstanceNotifier Instance = new InstanceNotifier();

        private readonly MessageSessionManager _sessions = new MessageSessionManager();

        public async Task StartAsync(ListenAddressConfigurationElementCollection listenAddresses)
        {
            Logger.Debug("Opening instance notifier sockets...");
            foreach(ListenAddressConfigurationElement listenAddress in listenAddresses) {
                Socket listener = NetUtil.CreateMulticastListener(
                    listenAddress.InterfaceAddress,
                    listenAddress.Port,
                    listenAddress.MulticastGroupIPAddress,
                    listenAddress.MulticastTTL);

                NetworkSession sender = new InstanceNotifierSession(listener);
                await sender.ConnectMulticastAsync(listenAddress.MulticastGroupIPAddress, listenAddress.Port, listenAddress.MulticastTTL).ConfigureAwait(false);
                _sessions.Add(sender);
            }
        }

        public void Stop()
        {
            Logger.Debug("Disconnecting instance notifier sessions...");
            _sessions.DisconnectAllAsync().Wait();
        }

        public async Task RunAsync()
        {
            await _sessions.PollAndReadAllAsync(100).ConfigureAwait(false);
            _sessions.Cleanup();
        }

        public async Task StartupAsync()
        {
            await _sessions.BroadcastMessageAsync(new StartupMessage()
                {
                    ServiceName = Chat.ServiceId,
                    ServiceId = Chat.UniqueId.ToString(),
                }).ConfigureAwait(false);
        }

        public async Task ShutdownAsync()
        {
            await _sessions.BroadcastMessageAsync(new ShutdownMessage()
                {
                    ServiceName = Chat.ServiceId,
                    ServiceId = Chat.UniqueId.ToString(),
                }).ConfigureAwait(false);
        }

        private InstanceNotifier()
        {
        }
    }
}
