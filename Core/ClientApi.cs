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

#region Notification Properties
        public const string NOTIFY_CONNECTING = "Connecting";
        public const string NOTIFY_CONNECTED = "Connected";
#endregion

#region Network Events
        public delegate void OnConnectSuccessHandler();
        public event OnConnectSuccessHandler OnConnectSuccess;
        
        public delegate void OnConnectFailedHandler(SocketError error);
        public event OnConnectFailedHandler OnConnectFailed;

        public delegate void OnDisconnectHandler();
        public event OnDisconnectHandler OnDisconnect;
#endregion

        public delegate void OnErrorHandler(string error);
        public event OnErrorHandler OnError;

#region Network Properties
        private volatile bool _connecting = false;
        public bool Connecting
        {
            get { return _connecting; }
            private set
            {
                lock(_lock) {
                    _connecting = value;
                    NotifyPropertyChanged(NOTIFY_CONNECTING);
                    NotifyPropertyChanged(NOTIFY_CONNECTED);
                }
            } 
        }
        public bool Connected { get { return _socketState.Connected; } }

        public string Host { get; private set; }

        private volatile SocketState _socketState = new SocketState();
        private Socket Socket { get { return _socketState.Socket; } set { _socketState.Socket = value; } }
        public BufferedSocketReader Reader { get { return _socketState.Reader; } }
#endregion

        public string Ticket { get; protected set; }

#region Network Methods
        private void OnConnectAsyncFailed(SocketError error)
        {
            lock(_lock) {
                _logger.Error("Connect failed: " + error);

                Host = null;
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

                Socket = socket;
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

                Host = host;
                _logger.Info("Connecting to " + Host + ":" + port + "...");

                Connecting = true;
            }

            AsyncConnectEventArgs args = new AsyncConnectEventArgs();
            args.OnConnectFailed += OnConnectAsyncFailed;
            args.OnConnectSuccess += OnConnectAsyncSuccess;
            NetUtil.ConnectAsync(Host, port, args);
        }

        public void Disconnect()
        {
            if(_socketState.HasSocket) {
                lock(_lock) {
                    _logger.Info("Disconnecting...");
                    _socketState.ShutdownAndClose(false);
                }

                if(null != OnDisconnect) {
                    OnDisconnect();
                }
            }

            lock(_lock) {
                Host = null;
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
                    if(Socket.Poll(100, SelectMode.SelectRead)) {
                        int len = Reader.Read();
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

        public void SendMessage(IMessage message, IMessageFormatter formatter)
        {
            if(!Connected) {
                return;
            }

            NetworkMessage packet = new NetworkMessage();	
            packet.Payload = message;

            byte[] bytes = packet.Serialize(formatter);
            lock(_lock) {
                _logger.Debug("Sending " + bytes.Length + " bytes");
                Socket.Send(bytes);
            }
        }
#endregion

        public void Error(string error)
        {
            _logger.Error("Encountered an error: " + error);
            Disconnect();

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
