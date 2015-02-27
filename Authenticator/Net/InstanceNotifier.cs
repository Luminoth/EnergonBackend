using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Authenticator.MessageHandlers;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Notification;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Net.Sockets;

using log4net;

namespace EnergonSoftware.Authenticator.Net
{
    internal sealed class InstanceNotifier
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(InstanceNotifier));

        public static readonly InstanceNotifier Instance = new InstanceNotifier();

        private readonly SessionManager _sessions = new SessionManager();

        public async Task StartAsync(ListenAddressConfigurationElementCollection listenAddresses)
        {
            Logger.Debug("Opening instance notifier sockets...");
            foreach(ListenAddressConfigurationElement listenAddress in listenAddresses) {
                Socket listener = NetUtil.CreateMulticastListener(
                    listenAddress.InterfaceAddress,
                    listenAddress.Port,
                    listenAddress.MulticastGroupIPAddress,
                    listenAddress.MulticastTTL);

                Session sender = new InstanceNotifierSession(new SocketState(listener));
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
            await _sessions.PollAndRunAsync().ConfigureAwait(false);
            _sessions.Cleanup();
        }

        public async Task StartupAsync()
        {
            await _sessions.BroadcastMessageAsync(new StartupMessage()
                {
                    ServiceName = Authenticator.ServiceId,
                    ServiceId = Authenticator.UniqueId.ToString(),
                }).ConfigureAwait(false);
        }

        public async Task ShutdownAsync()
        {
            await _sessions.BroadcastMessageAsync(new ShutdownMessage()
                {
                    ServiceName = Authenticator.ServiceId,
                    ServiceId = Authenticator.UniqueId.ToString(),
                }).ConfigureAwait(false);
        }

        public async Task AuthenticatingAsync(string accountName, EndPoint endpoint)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public async Task AuthenticatedAsync(string accountName, string sessionId, EndPoint endpoint)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        private InstanceNotifier()
        {
        }
    }
}
