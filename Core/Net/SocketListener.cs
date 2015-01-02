using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.Configuration;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public sealed class SocketListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SocketListener));

        private readonly List<Socket> _listenSockets = new List<Socket>();
        private readonly ISessionFactory _factory;

        public int MaxConnections { get; set; }
        public int SocketBacklog { get; set; }

        private SocketListener()
        {
            MaxConnections = -1;
            SocketBacklog = 10;
        }

        public SocketListener(ISessionFactory factory) : this()
        {
            _factory = factory;
        }

        public void CreateSockets(ListenAddressConfigurationElementCollection listenAddresses)
        {
            foreach(ListenAddressConfigurationElement listenAddress in listenAddresses) {
                Logger.Info("Listening on address " + listenAddress + "...");

                try {
                    IPEndPoint endpoint = new IPEndPoint(listenAddress.InterfaceAddress, listenAddress.Port);
                    Socket socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    socket.Bind(endpoint);
                    socket.Listen(SocketBacklog);
                    _listenSockets.Add(socket);
                } catch(SocketException e) {
                    Logger.Error("Exception creating socket!", e);
                }
            }
        }

        public void CloseSockets()
        {
            Logger.Info("Closing listen sockets...");
            Parallel.ForEach<Socket>(_listenSockets, socket => socket.Close());
            _listenSockets.Clear();
        }

        public void Poll(SessionManager manager)
        {
            Parallel.ForEach<Socket>(_listenSockets, socket =>
                {
                    try {
                        if(socket.Poll(100, SelectMode.SelectRead)) {
                            Socket remote = socket.Accept();
                            Logger.Info("New connection from " + remote.RemoteEndPoint);

                            if(MaxConnections >= 0 && manager.Count >= MaxConnections) {
                                Logger.Info("Max connections exceeded, denying new connection!");
                                remote.Close();
                            } else {
                                Logger.Debug("Allowing new connection...");
                                Session session = _factory.Create(remote);
                                manager.Add(session);
                            }
                        }
                    } catch(SocketException e) {
                        Logger.Error("Exception polling sockets!", e);
                    }
                }
            );
        }
    }
}
