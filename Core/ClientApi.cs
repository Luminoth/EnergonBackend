using System;
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

        private object _lock = new object();

#region Network Events
        public delegate void OnConnectSuccessHandler();
        public event OnConnectSuccessHandler OnConnectSuccess;
        
        public delegate void OnConnectFailedHandler(SocketError error);
        public event OnConnectFailedHandler OnConnectFailed;

        public delegate void OnDisconnectHandler();
        public event OnDisconnectHandler OnDisconnect;
#endregion

#region Network Properties
        private volatile bool _connecting = false;
        private volatile string _host;
        private volatile Socket _socket;
        private volatile BufferedSocketReader _reader;
        private volatile IMessageFormatter _formatter = new BinaryMessageFormatter();

        public bool Connecting
        {
            get { return _connecting; }
            private set
            {
                lock(_lock) {
                    _connecting = value;
                    NotifyPropertyChanged("Connecting");
                    NotifyPropertyChanged("Connected");
                }
            } 
        }

        public bool Connected { get { return null != _socket && _socket.Connected; } }

        public string Host { get { return _host; } }

        public BufferedSocketReader Reader { get { return _reader; } }
        public IMessageFormatter Formatter { get { return _formatter; } }
#endregion

#region Network Methods
        private void OnConnectAsyncFailed(SocketError error)
        {
            lock(_lock) {
                _logger.Error("Connect failed: " + error);

                _host = null;
                Connecting = false;
            }

            if(null != OnConnectFailed) {
                OnConnectFailed(error);
            }

            OnConnectFailed = null;
            OnConnectSuccess = null;
        }

        private void OnConnectAsyncSuccess(Socket socket)
        {
            lock(_lock) {
                _logger.Info("Connected to " + socket.RemoteEndPoint);

                _socket = socket;
                _reader = new BufferedSocketReader(_socket);
                Connecting = false;
            }

            if(null != OnConnectSuccess) {
                OnConnectSuccess();
            }

            OnConnectFailed = null;
            OnConnectSuccess = null;
        }

        public void ConnectAsync(string host, int port)
        {
            lock(_lock) {
                Disconnect();

                _host = host;
                _logger.Info("Connecting to " + Host + ":" + port + "...");

                Connecting = true;
            }

            AsyncConnectEventArgs args = new AsyncConnectEventArgs();
            args.OnConnectFailed += OnConnectAsyncFailed;
            args.OnConnectSuccess += OnConnectAsyncSuccess;
            NetUtil.ConnectAsync(host, port, args);
        }

        public void Disconnect()
        {
            if(null != _socket) {
                lock(_lock) {
                    _logger.Info("Disconnecting...");
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Disconnect(false);
                    _socket.Close();
                    _socket = null;
                    _reader = null;
                }

                if(null != OnDisconnect) {
                    OnDisconnect();
                }
            }

            lock(_lock) {
                _host = null;
                Connecting = false;
            }
        }

        public bool Poll()
        {
            if(!Connected) {
                return false;
            }

            lock(_lock) {
                // TODO: this needs to be a while loop
                try {
                    if(_socket.Poll(100, SelectMode.SelectRead)) {
                        int len = _reader.Read();
                        if(0 == len) {
                            Error("End of stream!");
                            return false;
                        }
                        _logger.Debug("Read " + len + " bytes");
                    }
                } catch(SocketException e) {
                    Error(e);
                    return false;
                }
            }

            return true;
        }

        public void SendMessage(IMessage message)
        {
            if(!Connected) {
                return;
            }

            NetworkMessage packet = new NetworkMessage();	
            packet.Payload = message;

            byte[] bytes = packet.Serialize(_formatter);
            lock(_lock) {
                _logger.Debug("Sending " + bytes.Length + " bytes");
                _socket.Send(bytes);
            }
        }
#endregion

        protected void Error(string error)
        {
            _logger.Error("Encountered an error: " + error);
            Disconnect();
        }

        protected void Error(Exception error)
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
