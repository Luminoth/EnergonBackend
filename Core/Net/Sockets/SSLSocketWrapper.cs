﻿using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.Net.Sockets
{
    /// <summary>
    /// Wraps a socket to allow for mid-stream SSL handshakes.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public sealed class SSLSocketWrapper : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SSLSocketWrapper));

        /// <summary>
        /// Gets the socket identifier.
        /// </summary>
        /// <value>
        /// The socket identifier.
        /// </value>
        public int SocketId { get { return null != _socket ? _socket.Handle.ToInt32() : -1; } }

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

#region Socket Properties
        /// <summary>
        /// Gets a value indicating whether this socket is connecting.
        /// </summary>
        /// <value>
        /// <c>true</c> if this socket is connecting; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnecting { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this socket is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instancesocket is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected { get { return null != _socket && _socket.Connected; } }

        /// <summary>
        /// Gets a value indicating whether this socket is encrypted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this socket is encrypted; otherwise, <c>false</c>.
        /// </value>
        public bool IsEncrypted { get { return null != _sslStream && _sslStream.IsEncrypted; } }

        /// <summary>
        /// Gets the socket remote end point.
        /// </summary>
        /// <value>
        /// The socket remote end point.
        /// </value>
        public EndPoint RemoteEndPoint { get { return null != _socket ? _socket.RemoteEndPoint : null; } }

        private Socket _socket;
#endregion

#region Stream Properties
        private NetworkStream _stream;

        private SslStream _sslStream;

        private Stream Stream { get { return _sslStream ?? (Stream)_stream; } }
#endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SSLSocketWrapper"/> class.
        /// </summary>
        public SSLSocketWrapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SSLSocketWrapper"/> class.
        /// </summary>
        /// <param name="socket">The socket to wrap.</param>
        public SSLSocketWrapper(Socket socket)
            : this()
        {
            _socket = socket;
            _stream = new NetworkStream(_socket);
        }

#region Dispose
        public void Dispose()
        {
            Dispose(true);
            ////GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(disposing) {
                _lock.Dispose();
            }
        }
#endregion

        /// <summary>
        /// Connects the socket to the given host:port with the given socket properties.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="socketType">Type of the socket.</param>
        /// <param name="protocolType">Type of the socket protocol.</param>
        /// <param name="useIPv6">if set to <c>true</c> use IPv6, otherwise use IPv4.</param>
        /// <returns></returns>
        public async Task ConnectAsync(string host, int port, SocketType socketType, ProtocolType protocolType, bool useIPv6)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                IsConnecting = true;

                _socket = await NetUtil.ConnectAsync(host, port, socketType, protocolType, useIPv6).ConfigureAwait(false);
                _stream = new NetworkStream(_socket);
            } finally {
                IsConnecting = false;
                _lock.Release();
            }
        }

        /// <summary>
        /// Connects the socket to the given multicast group (IP address).
        /// </summary>
        /// <param name="group">The multicast group to join.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="ttl">The multicast time to live (TTL).</param>
        public async Task ConnectMulticastAsync(IPAddress group, int port, int ttl)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                IsConnecting = true;

                _socket = await NetUtil.ConnectMulticastAsync(group, port, ttl).ConfigureAwait(false);
                _stream = new NetworkStream(_socket);
            } finally {
                IsConnecting = false;
                _lock.Release();
            }
        }

        /// <summary>
        /// Disconnects the socket.
        /// </summary>
        /// <param name="reuseSocket">The disconnect reuseSocket value.</param>
        public async Task DisconnectAsync(bool reuseSocket)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                await _socket.ShutdownDisconnectCloseAsync(SocketShutdown.Both, reuseSocket).ConfigureAwait(false);
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Starts the client-side SSL handshake.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <param name="userCertificateValidationCallback">The user certificate validation callback.</param>
        /// <param name="enabledSslProtocols">The enabled SSL protocols.</param>
        public async Task StartClientSslAsync(string serverName, RemoteCertificateValidationCallback userCertificateValidationCallback, SslProtocols enabledSslProtocols)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                _sslStream = new SslStream(_stream, false, userCertificateValidationCallback, null);
                await _sslStream.AuthenticateAsClientAsync(serverName, null, enabledSslProtocols, true).ConfigureAwait(false);
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Starts the server-side SSL handshake.
        /// </summary>
        /// <param name="serverCertificate">The server certificate.</param>
        /// <param name="enabledSslProtocols">The enabled SSL protocols.</param>
        /// <returns></returns>
        public async Task StartServerSslAsync(X509Certificate serverCertificate, SslProtocols enabledSslProtocols)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                _sslStream = new SslStream(_stream, false);
                await _sslStream.AuthenticateAsServerAsync(serverCertificate, false, enabledSslProtocols, true);
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Polls the socket and reads all the available data.
        /// </summary>
        /// <param name="microSeconds">The microsecond poll timeout.</param>
        /// <param name="stream">The stream to read into.</param>
        /// <returns>The number of bytes read.</returns>
        public async Task<int> PollAndReadAllAsync(int microSeconds, Stream stream)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                int total = 0;
                while(_socket.Poll(microSeconds, SelectMode.SelectRead)) {
                    if(_socket.Available <= 0) {
                        return total > 0 ? total : -1;
                    }

                    // read from the network stream, not the socket
                    byte[] buffer = new byte[_socket.Available];
                    int len = await Stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    if(len <= 0) {
                        return total > 0 ? total : -1;
                    }

                    total += len;
                    await stream.WriteAsync(buffer, 0, len);
                }

                return total;
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Writes data to the socket.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public async Task WriteAsync(byte[] data, int offset, int count)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                Logger.Debug("Writing buffer to SSL socket:");
                Logger.Debug(Utils.HexDump(data, offset, count));

                await Stream.WriteAsync(data, offset, count).ConfigureAwait(false);
                await Stream.FlushAsync().ConfigureAwait(false);
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Writes data to the socket.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public async Task WriteAsync(MemoryStream stream)
        {
            byte[] buffer = stream.ToArray();
            await WriteAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Closes this socket.
        /// Does NOT lock it.
        /// </summary>
        public void Close()
        {
            if(null != _sslStream) {
                _sslStream.Close();
                _sslStream = null;
            }

            if(null != _stream) {
                _stream.Close();
                _stream = null;
            }

            // TODO: this may be redundant
            if(null != _socket) {
                _socket.Close();
                _socket = null;
            }
        }
    }
}
