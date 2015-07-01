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
using EnergonSoftware.Core.Util;

using log4net;

// TODO: decouple this from the message concept

namespace EnergonSoftware.Core.Net.Sessions
{
    public abstract class NetworkSession : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NetworkSession));

#region Id Generator
        private static int _nextId;
        private static int NextId { get { return ++_nextId; } }
#endregion

#region Events
        public event EventHandler<ConnectedEventArgs> ConnectedEvent;
        public event EventHandler<DisconnectedEventArgs> DisconnectedEvent;
        public event EventHandler<Util.ErrorEventArgs> ErrorEvent;
        public event EventHandler<DataReceivedEventArgs> DataReceivedEvent;
#endregion

        public int Id { get; private set; }

        public abstract string Name { get; }

#region Network Properties
        public bool IsConnecting { get { return _socket.IsConnecting; } }

        public bool IsConnected { get { return _socket.IsConnected; } }

        public bool IsEncrypted { get { return _socket.IsEncrypted; } }

        public EndPoint RemoteEndPoint { get { return _socket.RemoteEndPoint; } }

        // ReSharper disable once InconsistentNaming
        public long LastMessageTimeMS { get; private set; }

        public long Timeout { get; set; }
        public bool TimedOut { get { return Timeout >= 0 && Time.CurrentTimeMs >= (LastMessageTimeMS + Timeout); } }

        private SSLSocketWrapper _socket = new SSLSocketWrapper();
#endregion

#region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _socket.Dispose();
            }
        }
#endregion

        public async Task ConnectAsync(string host, int port, SocketType socketType, ProtocolType protocolType, bool useIPv6)
        {
            // TODO: disconnect first?

            Logger.Info("Session " + Id + " connecting to " + host + ":" + port + "...");
            _socket = new SSLSocketWrapper(await NetUtil.ConnectAsync(host, port, socketType, protocolType, useIPv6).ConfigureAwait(false));

            if(IsConnected) {
                if(null != ConnectedEvent) {
                    ConnectedEvent(this, new ConnectedEventArgs());
                }
            } else {
                // TODO: error?
            }
        }

        public async Task ConnectMulticastAsync(IPAddress group, int port, int ttl)
        {
            // TODO: disconnect first?

            Logger.Info("Session " + Id + " connecting to multicast group " + group + ":" + port + "...");
            _socket = new SSLSocketWrapper(await NetUtil.ConnectMulticastAsync(group, port, ttl).ConfigureAwait(false));

            if(IsConnected) {
                if(null != ConnectedEvent) {
                    ConnectedEvent(this, new ConnectedEventArgs());
                }
            } else {
                // TODO: error?
            }
        }

        public async Task DisconnectAsync()
        {
            Logger.Info("Session " + Id + " disconnecting...");
            await DisconnectAsync(string.Empty).ConfigureAwait(false);
        }

        public async Task DisconnectAsync(string reason)
        {
            try {
                if(!IsConnected) {
                    return;
                }

                Logger.Info("Session " + Id + " disconnecting: " + reason);
                await _socket.DisconnectAsync(false).ConfigureAwait(false);

                if(null != DisconnectedEvent) {
                    DisconnectedEvent(this, new DisconnectedEventArgs() { Reason = reason });
                }
            } catch(SocketException e) {
                Logger.Error("Error disconnecting socket!", e);
            }
        }

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

        public async Task PollAndReadAllAsync(int microSeconds)
        {
            if(!IsConnected) {
                return;
            }

            using(MemoryStream stream = new MemoryStream()) {
                int count = await _socket.PollAndReadAllAsync(microSeconds, stream).ConfigureAwait(false);
                if(count < 0) {
                    Logger.Warn("Session " + Id + " remote disconnected!");
                    if(null != DisconnectedEvent) {
                        DisconnectedEvent(this, new DisconnectedEventArgs() { Reason = Resources.DisconnectSocketClosed });
                    }

                    return;
                } else if(0 == count) {
                    return;
                }

                OnDataReceived(stream.ToArray());
            }
        }

        protected void OnDataReceived(byte[] data)
        {
            Logger.Debug("Session " + Id + " read " + data.Length + " bytes");

            if(null != DataReceivedEvent) {
                DataReceivedEvent(this,
                    new DataReceivedEventArgs()
                    {
                        Count = data.Length,
                        Data = data,
                    });
            }
        }

        public async Task SendAsync(byte[] data, int offset, int count)
        {
            try {
                if(!IsConnected) {
                    return;
                }

                await _socket.WriteAsync(data, offset, count).ConfigureAwait(false);
            } catch(SocketException e) {
                InternalErrorAsync(Resources.ErrorSendingSessionData, e).Wait();
            }
        }

        public async Task CopyAsync(MemoryStream stream)
        {
            try {
                if(!IsConnected) {
                    return;
                }

                await _socket.CopyAsync(stream).ConfigureAwait(false);
            } catch(SocketException e) {
                InternalErrorAsync(Resources.ErrorSendingSessionData, e).Wait();
            }
        }

#region Internal Errors
        public async Task InternalErrorAsync(string error)
        {
            Logger.Error("Session " + Id + " encountered an internal error: " + error);
            await DisconnectAsync(Resources.DisconnectInternalError).ConfigureAwait(false);

            if(null != ErrorEvent) {
                ErrorEvent(this, new Util.ErrorEventArgs() { Error = error });
            }
        }

        public async Task InternalErrorAsync(string error, Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an internal error: " + error, ex);
            await DisconnectAsync(Resources.DisconnectInternalError).ConfigureAwait(false);

            if(null != ErrorEvent) {
                ErrorEvent(this, new Util.ErrorEventArgs() { Error = error, Exception = ex });
            }
        }

        public async Task InternalErrorAsync(Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an internal error", ex);
            await DisconnectAsync(Resources.DisconnectInternalError).ConfigureAwait(false);

            if(null != ErrorEvent) {
                ErrorEvent(this, new Util.ErrorEventArgs() { Exception = ex });
            }
        }
#endregion

#region Errors
        public async Task ErrorAsync(string error)
        {
            Logger.Error("Session " + Id + " encountered an error: " + error);
            await DisconnectAsync(error).ConfigureAwait(false);

            if(null != ErrorEvent) {
                ErrorEvent(this, new Util.ErrorEventArgs() { Error = error });
            }
        }

        public async Task ErrorAsync(string error, Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an error: " + error, ex);
            await DisconnectAsync(error).ConfigureAwait(false);

            if(null != ErrorEvent) {
                ErrorEvent(this, new Util.ErrorEventArgs() { Error = error, Exception = ex });
            }
        }

        public async Task ErrorAsync(Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an error", ex);
            await DisconnectAsync(ex.Message).ConfigureAwait(false);

            if(null != ErrorEvent) {
                ErrorEvent(this, new Util.ErrorEventArgs() { Exception = ex });
            }
        }
#endregion

        protected NetworkSession()
        {
            Id = NextId;

            Timeout = -1;
        }

        protected NetworkSession(Socket socket) : this()
        {
            _socket = new SSLSocketWrapper(socket);
        }
    }
}
