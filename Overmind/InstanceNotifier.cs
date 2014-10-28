using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Overmind
{
    sealed class InstanceNotifier
    {
#region Singleton
        private static InstanceNotifier _instance = new InstanceNotifier();
        public static InstanceNotifier Instance { get { return _instance; } }
#endregion

        private static readonly ILog _logger = LogManager.GetLogger(typeof(InstanceNotifier));

#region Network Properties
        private List<Socket> _listenSockets = new List<Socket>();

        private IMessageFormatter _formatter = new BinaryMessageFormatter();
#endregion

#region Network Methods
        public bool CreateSockets()
        {
            ListenAddressesConfigurationSection config = (ListenAddressesConfigurationSection)ConfigurationManager.GetSection("instanceNotifierAddresses");
            if(null == config || config.ListenAddresses.Count < 1) {
                _logger.Error("No configured instance notifier addresses!");
                return false;
            }

            lock(_listenSockets) {
                foreach(ListenAddressConfigurationElement listenAddress in config.ListenAddresses) {
                    IPEndPoint endpoint = new IPEndPoint(listenAddress.IPAddress, listenAddress.Port);
                    _logger.Info("Instance notifier listening on endpoint " + endpoint + "...");

                    Socket socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(endpoint);
                    socket.Listen(Int32.Parse(ConfigurationManager.AppSettings["socketBacklog"]));
                    _listenSockets.Add(socket);
                }
            }

            return true;
        }

        public void CloseSockets()
        {
            _logger.Info("Closing instance notifier listen sockets...");
            lock(_listenSockets) {
                _listenSockets.ForEach(s => s.Close());
                _listenSockets.Clear();
            }
        }

        private void PollListenSocket(Socket socket)
        {
            if(socket.Poll(100, SelectMode.SelectRead)) {
                // TODO: do something
            }
        }

        public void Poll()
        {
            lock(_listenSockets) {
                _listenSockets.ForEach(s => PollListenSocket(s));
            }
        }
#endregion

        private InstanceNotifier()
        {
        }
    }
}
