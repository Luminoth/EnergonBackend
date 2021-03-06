﻿using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages.Notification;
using EnergonSoftware.Backend.Net.Sessions;
using EnergonSoftware.Backend.Packet;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Serialization.Formatters;

using log4net;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class InstanceNotifier
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(InstanceNotifier));

        public static readonly InstanceNotifier Instance = new InstanceNotifier();

        private readonly MessageNetworkSessionManager _sessionManager = new MessageNetworkSessionManager();

        private string MessageFormatterType => BinaryNetworkFormatter.FormatterType;

        private string PacketType => NetworkPacket.PacketType;

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
                await _sessionManager.AddAsync(sender).ConfigureAwait(false);
            }
        }

        public void Stop()
        {
            Logger.Debug("Disconnecting instance notifier sessions...");
            _sessionManager.DisconnectAllAsync().Wait();
        }

        public async Task RunAsync()
        {
            await _sessionManager.PollAndReceiveAllAsync(100).ConfigureAwait(false);
            await _sessionManager.CleanupAsync().ConfigureAwait(false);
        }

        public async Task StartupAsync()
        {
            await _sessionManager.BroadcastAsync(new StartupMessage()
                {
                    ServiceName = Chat.ServiceId,
                    ServiceId = Chat.UniqueId.ToString(),
                }, MessageFormatterType, PacketType).ConfigureAwait(false);
        }

        public async Task ShutdownAsync()
        {
            await _sessionManager.BroadcastAsync(new ShutdownMessage()
                {
                    ServiceName = Chat.ServiceId,
                    ServiceId = Chat.UniqueId.ToString(),
                }, MessageFormatterType, PacketType).ConfigureAwait(false);
        }

        private InstanceNotifier()
        {
        }
    }
}
