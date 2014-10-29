using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

using log4net;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Core
{
    public class ClientApi
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ClientApi));

#region Network Events
        public delegate void OnConnectSuccessHandler(int socketId);
        public event OnConnectSuccessHandler OnConnectSuccess;
        
        public delegate void OnConnectFailedHandler(int socketId, SocketError error);
        public event OnConnectFailedHandler OnConnectFailed;

        public delegate void OnDisconnectHandler(int socketId);
        public event OnDisconnectHandler OnDisconnect;
#endregion

        public delegate void OnSocketErrorHandler(int socketId, string error);
        public event OnSocketErrorHandler OnSocketError;

        public delegate void OnErrorHandler(string error);
        public event OnErrorHandler OnError;

#region Network Properties
        private Dictionary<int, SocketState> _sockets = new Dictionary<int,SocketState>();
#endregion

        public string Ticket { get; protected set; }

#region Network Methods
        public SocketState GetSocketState(int socketId)
        {
            if(!_sockets.ContainsKey(socketId)) {
                return null;
            }
            return _sockets[socketId];
        }

        public BufferedSocketReader GetSocketReader(int socketId)
        {
            if(!_sockets.ContainsKey(socketId)) {
                return null;
            }
            return _sockets[socketId].Reader;
        }

        private void OnConnectAsyncFailedCallback(int socketId, SocketError error)
        {
            _logger.Error("Socket " + socketId + " connect failed: " + error);

            SocketState socketState = null;
            lock(_sockets) {
                socketState = GetSocketState(socketId);
                _sockets.Remove(socketId);
            }

            if(null == socketState) {
                _logger.Error("No such socket for connect failed: " + socketId);
                return;
            }

            lock(socketState) {
                socketState.Reset();
            }

            if(null != OnConnectFailed) {
                OnConnectFailed(socketId, error);
            }
        }

        private void OnConnectAsyncSuccessCallback(int socketId, Socket socket)
        {
            _logger.Info("Connected socket " + socketId + " to " + socket.RemoteEndPoint);

            SocketState socketState = null;
            lock(_sockets) {
                socketState = GetSocketState(socketId);
            }

            if(null == socketState) {
                _logger.Error("No such socket for connect success: " + socketId);
                return;
            }

            socketState.Socket = socket;
            socketState.Connecting = false;

            if(null != OnConnectSuccess) {
                OnConnectSuccess(socketId);
            }
        }

        public int ConnectAsync(string host, int port)
        {
            SocketState state = new SocketState();
            state.Host = host;
            state.Port = port;
            state.Connecting = true;
            lock(_sockets) {
                // TODO: handle overwrites?
                _sockets[state.Id] = state;
            }

            _logger.Info("Connecting to " + host + ":" + port + " with socket " + state.Id + "...");

            AsyncConnectEventArgs args = new AsyncConnectEventArgs(state.Id);
            args.OnConnectFailed += OnConnectAsyncFailedCallback;
            args.OnConnectSuccess += OnConnectAsyncSuccessCallback;
            NetUtil.ConnectAsync(host, port, args);

            return state.Id;
        }

        public void Disconnect(int socketId)
        {
            SocketState socketState = null;
            lock(_sockets) {
                socketState = GetSocketState(socketId);
                _sockets.Remove(socketId);
            }

            if(null == socketState) {
                _logger.Error("No such socket for disconnect: " + socketId);
                return;
            }

            if(socketState.HasSocket) {
                lock(socketState) {
                    _logger.Info("Disconnecting socket " + socketState.Id + "...");
                    socketState.ShutdownAndClose(false);
                }

                if(null != OnDisconnect) {
                    OnDisconnect(socketId);
                }
            }
        }

        public bool Poll(int socketId)
        {
            SocketState socketState = null;
            lock(_sockets) {
                socketState = GetSocketState(socketId);
            }

            if(null == socketState) {
                _logger.Error("No such socket for poll: " + socketId);
                return false;
            }

            if(!socketState.Connected) {
                return false;
            }

            lock(socketState) {
                // TODO: this needs to be a while loop
                try {
                    if(socketState.Socket.Poll(100, SelectMode.SelectRead)) {
                        int len = socketState.Reader.Read();
                        if(0 == len) {
                            SocketError(socketId, "End of stream!");
                            return false;
                        }
                        _logger.Debug("Read " + len + " bytes");
                    }
                } catch(SocketException e) {
                    SocketError(socketId, e);
                    return false;
                }
            }

            return true;
        }

        public void SendMessage(int socketId, IMessage message, IMessageFormatter formatter)
        {
            SocketState socketState = null;
            lock(_sockets) {
                socketState = GetSocketState(socketId);
            }

            if(null == socketState) {
                _logger.Error("No such socket for sending: " + socketId);
                return;
            }

            if(!socketState.Connected) {
                return;
            }

            NetworkMessage packet = new NetworkMessage();	
            packet.Payload = message;

            byte[] bytes = packet.Serialize(formatter);
            lock(socketState) {
                _logger.Debug("Socket " + socketId + " sending " + bytes.Length + " bytes");
                socketState.Socket.Send(bytes);
            }
        }
#endregion

        public void SocketError(int socketId, string error)
        {
            _logger.Error("Socket " + socketId + " encountered an error: " + error);
            Disconnect(socketId);

            OnSocketError(socketId, error);
        }

        public void SocketError(int socketId, Exception error)
        {
            SocketError(socketId, error.Message);
        }

        public void Error(string error)
        {
            _logger.Error("Encountered an error: " + error);
            OnError(error);
        }

        public void Error(Exception error)
        {
            Error(error.Message);
        }

#region Property Notifier
        public event PropertyChangedEventHandler ApiPropertyChanged;
        private void NotifyPropertyChanged(/*[CallerMemberName]*/ string property/*=null*/)
        {
            if(null != ApiPropertyChanged) {
                ApiPropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion

        protected ClientApi()
        {
        }
    }
}
