using System;
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
    // ReSharper disable once InconsistentNaming
    public sealed class SSLSocketWrapper : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SSLSocketWrapper));

        public int Id { get { return null != _socket ? _socket.Handle.ToInt32() : -1; } }

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

#region Socket Properties
        public bool IsConnecting { get; private set; }

        public bool IsConnected { get { return null != _socket && _socket.Connected; } }

        public bool IsEncrypted { get { return null != _sslStream && _sslStream.IsEncrypted; } }

        public EndPoint RemoteEndPoint { get { return null != _socket ? _socket.RemoteEndPoint : null; } }

        private Socket _socket;
#endregion

#region Stream Properties
        private NetworkStream _stream;

        private SslStream _sslStream;

        private Stream Stream { get { return _sslStream ?? (Stream)_stream; } }
#endregion

        public SSLSocketWrapper()
        {
        }

        public SSLSocketWrapper(Socket socket) : this()
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

        public async Task DisconnectAsync(bool reuseSocket)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                await _socket.ShutdownDisconnectCloseAsync(SocketShutdown.Both, reuseSocket).ConfigureAwait(false);
            } finally {
                _lock.Release();
            }
        }

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

        public async Task WriteAsync(byte[] buffer, int offset, int count)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                Logger.Debug("Writing buffer to network stream:");
                Logger.Debug(Utils.HexDump(buffer, offset, count));

                await Stream.WriteAsync(buffer, offset, count).ConfigureAwait(false);
                await Stream.FlushAsync().ConfigureAwait(false);
            } finally {
                _lock.Release();
            }
        }

        public async Task WriteAsync(MemoryStream stream)
        {
            byte[] buffer = stream.ToArray();
            await WriteAsync(buffer, 0, buffer.Length);
        }

        // NOTE: does NOT lock the wrapper
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
