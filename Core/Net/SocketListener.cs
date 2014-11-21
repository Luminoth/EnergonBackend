using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core.Configuration;

namespace EnergonSoftware.Core.Net
{
    public sealed class SocketListener
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SocketListener));

        private List<Socket> _listenSockets = new List<Socket>();
        private ISessionFactory _factory;

        public int SocketBacklog { get; set; }

        private SocketListener()
        {
            SocketBacklog = 10;
        }

        public SocketListener(ISessionFactory factory)
        {
            SocketBacklog = 10;

            _factory = factory;
        }

        public void CreateSockets(ListenAddressConfigurationElementCollection listenAddresses)
        {
            foreach(ListenAddressConfigurationElement listenAddress in listenAddresses) {
                _logger.Info("Listening on address " + listenAddress + "...");

                IPEndPoint endpoint = new IPEndPoint(listenAddress.IPAddress, listenAddress.Port);
                Socket socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.Bind(endpoint);
                socket.Listen(SocketBacklog);
                _listenSockets.Add(socket);
            }
        }

        public void CloseSockets()
        {
            _logger.Info("Closing listen sockets...");
            _listenSockets.ForEach(s => s.Close());
            _listenSockets.Clear();
        }

        public void Poll(SessionManager manager)
        {
            _listenSockets.ForEach(socket =>
                {
                    if(socket.Poll(100, SelectMode.SelectRead)) {
                        Socket remote = socket.Accept();
                        _logger.Info("New connection from " + remote.RemoteEndPoint);

                        Session session = _factory.CreateSession(remote, manager);
                        manager.Add(session);
                    }
                }
            );
        }
    }
}
