﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Properties;

using log4net;

// TODO: decouple this from the session concept
// and use events instead

namespace EnergonSoftware.Core.Net.Sockets
{
    public sealed class UdpListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UdpListener));

        private readonly object _lock = new object();

        private readonly List<Socket> _listenSockets = new List<Socket>();
        private readonly INetworkSessionFactory _factory;

        public int MaxConnections { get; set; }

        private UdpListener()
        {
            MaxConnections = -1;
        }

        public UdpListener(INetworkSessionFactory factory) : this()
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
                        socket = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        socket.Bind(endpoint);
                        _listenSockets.Add(socket);
                    } catch(SocketException e) {
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

        private async Task PollAsync(Socket socket, NetworkSessionManager manager, int microSeconds)
        {
            // TODO: this probably doesn't work correctly
            try {
                if(socket.Poll(100, SelectMode.SelectRead)) {
                    Socket remote = await socket.AcceptFromAsync().ConfigureAwait(false);
                    if(manager.Contains(remote.RemoteEndPoint)) {
                        NetworkSession session = manager.Get(remote.RemoteEndPoint);
                        await session.PollAndReadAllAsync(microSeconds).ConfigureAwait(false);
                        return;
                    }

                    Logger.Info("New connection from " + remote.RemoteEndPoint);

                    if(MaxConnections >= 0 && manager.Count >= MaxConnections) {
                        Logger.Info("Max connections exceeded, denying new connection!");
                        remote.Close();
                    } else {
                        Logger.Debug("Allowing new connection...");
                        NetworkSession session = _factory.Create(remote);
                        await session.PollAndReadAllAsync(microSeconds).ConfigureAwait(false);
                        manager.Add(session);
                    }
                }
            } catch(SocketException e) {
                Logger.Error("Exception polling sockets!", e);
            }
        }

        public async Task PollAsync(NetworkSessionManager manager, int microSeconds)
        {
            List<Task> tasks = new List<Task>();
            lock(_lock) {
                _listenSockets.ForEach(socket => tasks.Add(PollAsync(socket, manager, microSeconds)));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
