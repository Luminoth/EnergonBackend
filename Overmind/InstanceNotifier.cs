using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Overmind
{
    sealed class InstanceNotifier
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(InstanceNotifier));

#region Singleton
        private static InstanceNotifier _instance = new InstanceNotifier();
        public static InstanceNotifier Instance { get { return _instance; } }
#endregion

        private List<SocketState> _sockets = new List<SocketState>();
        private IMessageFormatter _formatter = new BinaryMessageFormatter();

        public void CreateSockets(ListenAddressConfigurationElementCollection listenAddresses)
        {
            foreach(ListenAddressConfigurationElement listenAddress in listenAddresses) {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, listenAddress.Port);
                _logger.Info("Multicasting on endpoint " + endpoint + "...");

// TODO: support ipv6 here

                Socket socket = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.Bind(endpoint);

                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(listenAddress.IPAddress, IPAddress.Any));
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 1);
                _sockets.Add(new SocketState(socket));
            }
        }

        public void CloseSockets()
        {
            _logger.Info("Closing sockets...");
            _sockets.ForEach(s => s.ShutdownAndClose(true));
            _sockets.Clear();
        }

        public void Run()
        {
            _sockets.ForEach(s =>
                {
                    s.PollAndRead();

                    NetworkMessage message = NetworkMessage.Parse(s.Buffer, _formatter);
                    while(null != message) {
                        _logger.Debug("Instance notifier parsed message type: " + message.Payload.Type);
                        //processor.QueueMessage(this, message.Payload);
                        message = NetworkMessage.Parse(s.Buffer, _formatter);
                    }
                }
            );
        }

        private void SendMessage(IMessage message)
        {
            NetworkMessage packet = new NetworkMessage();
            packet.Payload = message;

            _logger.Debug("Multicasting network message: " + packet);

            byte[] bytes = packet.Serialize(_formatter);
            _logger.Debug("Instance notifier multicasting " + bytes.Length + " bytes");
            _sockets.ForEach(s => s.Send(bytes));
        }

        public void Login(string username, EndPoint endpoint)
        {
        }

        public void Logout(string username)
        {
        }

        private InstanceNotifier()
        {
        }
    }
}
