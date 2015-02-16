using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Properties;

using log4net;

namespace EnergonSoftware.Core.Net.Sockets
{
    public sealed class SocketListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SocketListener));

        private readonly object _lock = new object();

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
            if(null == listenAddresses) {
                throw new ArgumentNullException("listenAddresses");
            }

            lock(_lock) {
                foreach(ListenAddressConfigurationElement listenAddress in listenAddresses) {
                    Logger.Info("Listening on address " + listenAddress + "...");

                    Socket socket = null;
                    try {
                        IPEndPoint endpoint = new IPEndPoint(listenAddress.InterfaceAddress, listenAddress.Port);
                        socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        socket.Bind(endpoint);
                        socket.Listen(SocketBacklog);
                        _listenSockets.Add(socket);
                    } catch(Exception e) {
                        if(null != socket) {
                            socket.Dispose();
                        }

                        Logger.Error(Resources.ErrorCreatingSocket, e);
                    }
                }
            }
        }

        public void CloseSockets()
        {
            lock(_lock) {
                Logger.Info("Closing listen sockets...");
                _listenSockets.ForEach(socket => socket.Close());
                _listenSockets.Clear();
            }
        }

        private async Task PollAsync(Socket socket, SessionManager manager)
        {
            try {
                if(socket.Poll(100, SelectMode.SelectRead)) {
                    Socket remote = await socket.AcceptAsync().ConfigureAwait(false);
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

        public async Task PollAsync(SessionManager manager)
        {
            List<Task> tasks = new List<Task>();
            lock(_lock) {
                _listenSockets.ForEach(socket => tasks.Add(PollAsync(socket, manager)));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
