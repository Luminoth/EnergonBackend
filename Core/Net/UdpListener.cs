using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using EnergonSoftware.Core.Configuration;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public sealed class UdpListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UdpListener));

        private List<Socket> _listenSockets = new List<Socket>();
        private ISessionFactory _factory;

        private UdpListener()
        {
        }

        public UdpListener(ISessionFactory factory)
        {
            _factory = factory;
        }

        public void CreateSockets(ListenAddressConfigurationElementCollection listenAddresses)
        {
            foreach(ListenAddressConfigurationElement listenAddress in listenAddresses) {
                Logger.Info("Listening on address " + listenAddress + "...");

                IPEndPoint endpoint = new IPEndPoint(listenAddress.InterfaceAddress, listenAddress.Port);
                Socket socket = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.Bind(endpoint);
                _listenSockets.Add(socket);
            }
        }

        public void CloseSockets()
        {
            Logger.Info("Closing listen sockets...");
            _listenSockets.ForEach(s => s.Close());
            _listenSockets.Clear();
        }

        private Socket Accept(Socket socket, out byte[] data)
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remoteEndpoint = (EndPoint)sender;

            data = new byte[socket.Available];
            int len = socket.ReceiveFrom(data, ref remoteEndpoint);
            if(len <= 0) {
                return null;
            }

            Socket remote = new Socket(socket.AddressFamily, socket.SocketType, socket.ProtocolType);
            remote.Connect(remoteEndpoint);
            Logger.Info("New connection from " + remote.RemoteEndPoint);
            return remote;
        }

        public void Poll(SessionManager manager)
        {
            _listenSockets.ForEach(socket =>
                {
                    if(socket.Poll(100, SelectMode.SelectRead)) {
                        byte[] data;
                        Socket remote = Accept(socket, out data);
                        if(manager.Contains(remote.RemoteEndPoint)) {
                            manager.Get(remote.RemoteEndPoint).BufferData(data);
                            return;
                        }

                        Logger.Info("New connection from " + remote.RemoteEndPoint);

                        Session session = _factory.CreateSession(remote, manager);
                        session.BufferData(data);
                        manager.Add(session);
                    }
                }
            );
        }
    }
}
