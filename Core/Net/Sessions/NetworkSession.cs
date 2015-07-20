using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using EnergonSoftware.Core.Net.Sockets;
using EnergonSoftware.Core.Properties;

using log4net;

namespace EnergonSoftware.Core.Net.Sessions
{
    /// <summary>
    /// A network session.
    /// </summary>
    public abstract class NetworkSession : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NetworkSession));

#region Id Generator
        private static long _nextId;
        private static long NextId => ++_nextId;

#endregion

#region Events
        /// <summary>
        /// Occurs when the session is connected.
        /// </summary>
        public event EventHandler<ConnectedEventArgs> ConnectedEvent;

        /// <summary>
        /// Occurs when the session is disconnected.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> DisconnectedEvent;

        /// <summary>
        /// Occurs when an error occurs.
        /// </summary>
        public event EventHandler<ErrorEventArgs> ErrorEvent;

        /// <summary>
        /// Occurs when data is received.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceivedEvent;
#endregion

        /// <summary>
        /// Gets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public long Id { get; } = NextId;

        /// <summary>
        /// Gets the session name.
        /// </summary>
        /// <value>
        /// The session name.
        /// </value>
        public abstract string Name { get; }

#region Network Properties
        /// <summary>
        /// Gets a value indicating whether this session is connecting.
        /// </summary>
        /// <value>
        /// <c>true</c> if this session is connecting; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnecting => _socket.IsConnecting;

        /// <summary>
        /// Gets a value indicating whether this session is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this session is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected => _socket.IsConnected;

        /// <summary>
        /// Gets a value indicating whether this session is encrypted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this session is encrypted; otherwise, <c>false</c>.
        /// </value>
        public bool IsEncrypted => _socket.IsEncrypted;

        /// <summary>
        /// Gets the remote end point associated with this session.
        /// </summary>
        /// <value>
        /// The remote end point associated with this session.
        /// </value>
        public EndPoint RemoteEndPoint => _socket.RemoteEndPoint;

        /// <summary>
        /// Gets the last time this session sent data.
        /// </summary>
        /// <value>
        /// The last time this session sent data.
        /// </value>
        public DateTime LastSendTime { get; private set; } = DateTime.MaxValue;

        /// <summary>
        /// Gets the last time this session received data.
        /// </summary>
        /// <value>
        /// The last time this session received data.
        /// </value>
        public DateTime LastRecvTime { get; private set; } = DateTime.MaxValue;

        /// <summary>
        /// Gets or sets the session timeout in milliseconds.
        /// </summary>
        /// <value>
        /// The session timeout in milliseconds.
        /// </value>
        public long TimeoutMs { get; set; } = -1;

        /// <summary>
        /// Gets a value indicating whether the session has timed out.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the session timed out; otherwise, <c>false</c>.
        /// </value>
        public bool TimedOut => TimeoutMs >= 0 && (DateTime.Now.Subtract(LastRecvTime).Milliseconds > TimeoutMs);

        private SSLSocketWrapper _socket = new SSLSocketWrapper();
#endregion

#region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _socket.Dispose();
            }
        }
#endregion

        /// <summary>
        /// Connects the session to the given host:port using the given socket properties.
        /// </summary>
        /// <param name="host">The host to connect to (hostname or IP address).</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="socketType">Type of the socket.</param>
        /// <param name="protocolType">Type of the socket protocol.</param>
        /// <param name="useIPv6">if set to <c>true</c> use IPv6, otherwise use IPv4.</param>
        public async Task ConnectAsync(string host, int port, SocketType socketType, ProtocolType protocolType, bool useIPv6)
        {
            // TODO: disconnect first?

            Logger.Info("Session " + Id + " connecting to " + host + ":" + port + "...");

            Socket socket = await NetUtil.ConnectAsync(host, port, socketType, protocolType, useIPv6).ConfigureAwait(false);
            _socket = new SSLSocketWrapper(socket);

            if(IsConnected) {
                ConnectedEvent?.Invoke(this, new ConnectedEventArgs());
            } else {
                // TODO: error?
            }
        }

        /// <summary>
        /// Connects the session to the given multicast group (IP address).
        /// </summary>
        /// <param name="group">The multicast group to join.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="ttl">The multicast time to live (TTL).</param>
        public async Task ConnectMulticastAsync(IPAddress group, int port, int ttl)
        {
            // TODO: disconnect first?

            Logger.Info("Session " + Id + " connecting to multicast group " + group + ":" + port + "...");

            Socket socket = await NetUtil.ConnectMulticastAsync(group, port, ttl).ConfigureAwait(false);
            _socket = new SSLSocketWrapper(socket);

            if(IsConnected) {
                ConnectedEvent?.Invoke(this, new ConnectedEventArgs());
            } else {
                // TODO: error?
            }
        }

        /// <summary>
        /// Disconnects the session.
        /// </summary>
        public async Task DisconnectAsync()
        {
            Logger.Info("Session " + Id + " disconnecting...");
            await DisconnectAsync(string.Empty).ConfigureAwait(false);
        }

        /// <summary>
        /// Disconnects the session with the given reason.
        /// </summary>
        /// <param name="reason">The reason for the disconnect.</param>
        public async Task DisconnectAsync(string reason)
        {
            try {
                if(!IsConnected) {
                    return;
                }

                Logger.Info("Session " + Id + " disconnecting: " + reason);
                await _socket.DisconnectAsync(false).ConfigureAwait(false);

                DisconnectedEvent?.Invoke(this, new DisconnectedEventArgs
                    {
                        Reason = reason
                    }
                );
            } catch(SocketException e) {
                Logger.Error("Error disconnecting socket!", e);
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
            Logger.Info("Session " + Id + " starting client SSL handshake with serverName=" + serverName + "...");

            await _socket.StartClientSslAsync(serverName, userCertificateValidationCallback, enabledSslProtocols).ConfigureAwait(false);

            if(IsEncrypted) {
                Logger.Info("Session " + Id + " SSL handshake success!");
            } else {
                await DisconnectAsync(Resources.ErrorSSLHandshakeFailed).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Starts the server-side SSL handshake.
        /// </summary>
        /// <param name="serverCertificate">The server certificate.</param>
        /// <param name="enabledSslProtocols">The enabled SSL protocols.</param>
        public async Task StartServerSslAsync(X509Certificate serverCertificate, SslProtocols enabledSslProtocols)
        {
            Logger.Info("Session " + Id + " starting server SSL handshake...");

            await _socket.StartServerSslAsync(serverCertificate, enabledSslProtocols).ConfigureAwait(false);

            if(IsEncrypted) {
                Logger.Info("Session " + Id + " SSL handshake success!");
            } else {
                await DisconnectAsync(Resources.ErrorSSLHandshakeFailed).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Polls the session socket and reads all of the available data.
        /// </summary>
        /// <param name="microSeconds">The microsecond poll timeout.</param>
        public async Task PollAndReadAllAsync(int microSeconds)
        {
            if(!IsConnected) {
                return;
            }

            using(MemoryStream stream = new MemoryStream()) {
                int count = await _socket.PollAndReadAllAsync(microSeconds, stream).ConfigureAwait(false);
                if(count < 0) {
                    Logger.Warn("Session " + Id + " remote disconnected!");
                    DisconnectedEvent?.Invoke(this, new DisconnectedEventArgs
                        {
                            Reason = Resources.DisconnectSocketClosed
                        }
                    );
                    return;
                }

                if(0 == count) {
                    return;
                }

                byte[] data = stream.ToArray();
                OnDataReceived(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Called when data is received.
        /// </summary>
        /// <param name="data">The data that was received.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        protected void OnDataReceived(byte[] data, int offset, int count)
        {
            Logger.Debug("Session " + Id + " read " + count + " bytes");

            byte[] dataCopy = new byte[count];
            Array.Copy(data, offset, dataCopy, 0, count);

            LastRecvTime = DateTime.Now;
            DataReceivedEvent?.Invoke(this, new DataReceivedEventArgs
                {
                    Count = count,
                    Data = dataCopy,
                }
            );
        }

        /// <summary>
        /// Sends data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public async Task SendAsync(byte[] data, int offset, int count)
        {
            try {
                if(!IsConnected) {
                    return;
                }

                await _socket.WriteAsync(data, offset, count).ConfigureAwait(false);
                LastSendTime = DateTime.Now;
            } catch(SocketException e) {
                InternalErrorAsync(Resources.ErrorSendingSessionData, e).Wait();
            }
        }

        /// <summary>
        /// Sends data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public async Task SendAsync(MemoryStream stream)
        {
            try {
                if(!IsConnected) {
                    return;
                }

                await _socket.WriteAsync(stream).ConfigureAwait(false);
                LastSendTime = DateTime.Now;
            } catch(SocketException e) {
                InternalErrorAsync(Resources.ErrorSendingSessionData, e).Wait();
            }
        }

#region Internal Errors
        /// <summary>
        /// Called when an internal error has occurred.
        /// </summary>
        /// <param name="error">The error.</param>
        public async Task InternalErrorAsync(string error)
        {
            Logger.Error("Session " + Id + " encountered an internal error: " + error);
            await DisconnectAsync(Resources.DisconnectInternalError).ConfigureAwait(false);

            ErrorEvent?.Invoke(this, new ErrorEventArgs
                {
                    Error = error
                }
            );
        }

        /// <summary>
        /// Called when an internal error has occurred.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="ex">The exception.</param>
        public async Task InternalErrorAsync(string error, Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an internal error: " + error, ex);
            await DisconnectAsync(Resources.DisconnectInternalError).ConfigureAwait(false);

            ErrorEvent?.Invoke(this, new ErrorEventArgs
                {
                    Error = error,
                    Exception = ex
                }
            );
        }

        /// <summary>
        /// Called when an internal error has occurred.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public async Task InternalErrorAsync(Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an internal error", ex);
            await DisconnectAsync(Resources.DisconnectInternalError).ConfigureAwait(false);

            ErrorEvent?.Invoke(this, new ErrorEventArgs
                {
                    Exception = ex
                }
            );
        }
#endregion

#region Errors
        /// <summary>
        /// Called when an error has occurred.
        /// </summary>
        /// <param name="error">The error.</param>
        public async Task ErrorAsync(string error)
        {
            Logger.Error("Session " + Id + " encountered an error: " + error);
            await DisconnectAsync(error).ConfigureAwait(false);

            ErrorEvent?.Invoke(this, new ErrorEventArgs
                {
                    Error = error
                }
            );
        }

        /// <summary>
        /// Called when an error has occurred.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="ex">The exception.</param>
        public async Task ErrorAsync(string error, Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an error: " + error, ex);
            await DisconnectAsync(error).ConfigureAwait(false);

            ErrorEvent?.Invoke(this, new ErrorEventArgs
                {
                    Error = error,
                    Exception = ex
                }
            );
        }

        /// <summary>
        /// Called when an error has occurred.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns></returns>
        public async Task ErrorAsync(Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an error", ex);
            await DisconnectAsync(ex.Message).ConfigureAwait(false);

            ErrorEvent?.Invoke(this, new ErrorEventArgs
                {
                    Exception = ex
                }
            );
        }
#endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkSession"/> class.
        /// </summary>
        protected NetworkSession()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkSession"/> class.
        /// </summary>
        /// <param name="socket">The already connected socket to wrap.</param>
        protected NetworkSession(Socket socket)
            : this()
        {
            _socket = new SSLSocketWrapper(socket);
        }
    }
}
