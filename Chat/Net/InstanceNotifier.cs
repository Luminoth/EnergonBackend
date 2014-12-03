﻿using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

using EnergonSoftware.Chat.MessageHandlers;
using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Notifications;
using EnergonSoftware.Core.Net;

using log4net;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class InstanceNotifier
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(InstanceNotifier));

#region Singleton
        private static InstanceNotifier _instance = new InstanceNotifier();
        public static InstanceNotifier Instance { get { return _instance; } }
#endregion

        private SessionManager _sessions = new SessionManager();

        public void Start(ListenAddressConfigurationElementCollection listenAddresses)
        {
            Logger.Debug("Starting instance notifier session manager...");
            _sessions.SessionTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
            _sessions.Start(new InstanceNotifierMessageHandlerFactory());

            Logger.Debug("Opening multicast sockets...");
            foreach(ListenAddressConfigurationElement listenAddress in listenAddresses) {
                Socket listener = NetUtil.CreateMulticastListener(listenAddress.InterfaceAddress, listenAddress.Port,
                    listenAddress.MulticastGroupIPAddress, listenAddress.MulticastTTL);

                Session sender = new InstanceNotifierSession(_sessions, new SocketState(listener));
                sender.ConnectMulticast(listenAddress.MulticastGroupIPAddress, listenAddress.Port, listenAddress.MulticastTTL);
                _sessions.Add(sender);
            }
        }

        public void Stop()
        {
            Logger.Debug("Stopping instance notifier session manager...");
            _sessions.Stop();
        }

        public void Run()
        {
            _sessions.PollAndRun();
            _sessions.Cleanup();
        }

        public void Startup()
        {
            StartupMessage message = new StartupMessage();
            message.ServiceName = Chat.ServiceId;
            message.ServiceId = Chat.UniqueId.ToString();
            _sessions.SendMessage(message);
        }

        public void Shutdown()
        {
            ShutdownMessage message = new ShutdownMessage();
            message.ServiceName = Chat.ServiceId;
            message.ServiceId = Chat.UniqueId.ToString();
            _sessions.SendMessage(message);
        }

        private InstanceNotifier()
        {
        }
    }
}