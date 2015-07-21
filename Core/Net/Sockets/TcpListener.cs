using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Properties;

using log4net;

namespace EnergonSoftware.Core.Net.Sockets
{
    /// <summary>
    /// Listens for new TCP connections.
    /// </summary>
    public sealed class TcpListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TcpListener));

#region Events
        /// <summary>
        /// Occurs when a new connection is accepted.
        /// </summary>
        public event EventHandler<NewConnectionEventArgs> NewConnectionEvent;
#endregion

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        private readonly List<Socket> _listenSockets = new List<Socket>();

        /// <summary>
        /// Gets or sets the socket backlog.
        /// </summary>
        /// <value>
        /// The socket backlog.
        /// </value>
        public int SocketBacklog { get; set; } = 10;

        /// <summary>
        /// Creates the listen sockets.
        /// </summary>
        /// <param name="listenAddresses">The listen addresses.</param>
        /// <exception cref="System.ArgumentNullException">listenAddresses</exception>
        public async Task CreateSocketsAsync(ListenAddressConfigurationElementCollection listenAddresses)
        {
            if(null == listenAddresses) {
                throw new ArgumentNullException(nameof(listenAddresses));
            }

            foreach(ListenAddressConfigurationElement listenAddress in listenAddresses) {
                Logger.Info($"Listening on address {listenAddress}...");

                Socket socket = null;
                try {
                    IPEndPoint endpoint = new IPEndPoint(listenAddress.InterfaceAddress, listenAddress.Port);
                    socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    socket.Bind(endpoint);
                    socket.Listen(SocketBacklog);
                } catch(SocketException e) {
                    socket?.Dispose();
                    Logger.Error(Resources.ErrorCreatingSocket, e);
                    return;
                }

                await _lock.WaitAsync().ConfigureAwait(false);
                try {
                    _listenSockets.Add(socket);
                } finally {
                    _lock.Release();
                }
            }

        }

        /// <summary>
        /// Closes the sockets.
        /// </summary>
        public async Task CloseSocketsAsync()
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                Logger.Info("Closing listen sockets...");
                _listenSockets.ForEach(socket => socket.Close());
                _listenSockets.Clear();
            } finally {
                _lock.Release();
            }
        }

        private async Task PollAsync(Socket socket, int microSeconds)
        {
            try {
                if(socket.Poll(microSeconds, SelectMode.SelectRead)) {
                    Socket remote = await socket.AcceptAsync().ConfigureAwait(false);
                    Logger.Info($"New connection from {remote.RemoteEndPoint}");

                    NewConnectionEvent?.Invoke(this, new NewConnectionEventArgs
                        {
                            Socket = remote
                        }
                    );
                }
            } catch(SocketException e) {
                Logger.Error("Exception polling sockets!", e);
            }
        }

        /// <summary>
        /// Polls all of the sockets for new connections.
        /// </summary>
        /// <param name="microSeconds">The microsecond poll timeout.</param>
        public async Task PollAsync(int microSeconds)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                List<Task> tasks = new List<Task>();
                _listenSockets.ForEach(socket => tasks.Add(PollAsync(socket, microSeconds)));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            } finally {
                _lock.Release();
            }
        }
    }
}
