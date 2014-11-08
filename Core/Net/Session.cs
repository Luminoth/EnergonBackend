using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Net
{
    public interface ISessionFactory
    {
        Session CreateSession();
        Session CreateSession(Socket socket);
    }

    public abstract class Session
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Session));

#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

#region Events
        public delegate void OnConnectSuccessHandler();
        public event OnConnectSuccessHandler OnConnectSuccess;
        
        public delegate void OnConnectFailedHandler(SocketError error);
        public event OnConnectFailedHandler OnConnectFailed;

        public delegate void OnDisconnectHandler();
        public event OnDisconnectHandler OnDisconnect;

        public delegate void OnSocketErrorHandler(string error);
        public event OnSocketErrorHandler OnSocketError;
#endregion

        public int Id { get; private set; }

#region Network Properties
        private SocketState _socketState;
        public EndPoint RemoteEndPoint { get { return _socketState.RemoteEndPoint ; } }

        public bool Connecting { get { return _socketState.Connecting; } }
        public bool Connected { get { return _socketState.Connected; } }

        public long LastMessageTime { get { return _socketState.LastMessageTime; } }

        public long Timeout { get; set; }
        public bool TimedOut { get { return Timeout < 0 ? false : Time.CurrentTimeMs >= (_socketState.LastMessageTime + Timeout); } }
#endregion

#region Message Properties
        private MessageHandler _messageHandler;
        public bool HasMessageHandler { get { return null != _messageHandler; } }

        protected abstract IMessageFormatter Formatter { get; }
#endregion

        public Session()
        {
            Id = NextId;

            _socketState = new SocketState();

            Timeout = -1;
        }

        public Session(Socket socket)
        {
            Id = NextId;

            _socketState = new SocketState(socket);

            Timeout = -1;
        }

        public void Run(MessageProcessor processor)
        {
            NetworkMessage message = NetworkMessage.Parse(_socketState.Buffer, Formatter);
            while(null != message) {
                _logger.Debug("Session " + Id + " parsed message type: " + message.Payload.Type);
                processor.QueueMessage(this, message.Payload);
                message = NetworkMessage.Parse(_socketState.Buffer, Formatter);
            }

            if(HasMessageHandler && _messageHandler.Finished) {
                _messageHandler = null;
            }

            OnRun();

            // TODO: we need a way to say "hey, this handler is taking WAY too long,
            // dump an error and kill the session"
        }

        protected abstract void OnRun();

        private void OnConnectAsyncFailedCallback(SocketError error)
        {
            _logger.Error("Session " + Id + " connect failed: " + error);

            Disconnect();

            if(null != OnConnectFailed) {
                OnConnectFailed(error);
            }
        }

        private void OnConnectAsyncSuccessCallback(Socket socket)
        {
            _logger.Info("Connected session " + Id + " to " + socket.RemoteEndPoint);

            _socketState.Socket = socket;
            _socketState.Connecting = false;

            if(null != OnConnectSuccess) {
                OnConnectSuccess();
            }
        }

        public void ConnectAsync(string host, int port)
        {
            _logger.Info("Session " + Id + " connecting to " + host + ":" + port + "...");
            _socketState.Connecting = true;

            AsyncConnectEventArgs args = new AsyncConnectEventArgs();
            args.OnConnectFailed += OnConnectAsyncFailedCallback;
            args.OnConnectSuccess += OnConnectAsyncSuccessCallback;
            NetUtil.ConnectAsync(host, port, args);
        }


        public void Disconnect()
        {
            if(Connected) {
                _logger.Info("Session " + Id + " disconnecting...");
                _socketState.ShutdownAndClose(false);

                if(null != OnDisconnect) {
                    OnDisconnect();
                }
            }
        }

        public void Poll()
        {
            if(!Connected) {
                return;
            }

            try {
                int len = _socketState.Poll();
                if(len < 0) {
                    Error("End of stream!");
                    return;
                } else if(len > 0) {
                    _logger.Debug("Session " + Id + " read " + len + " bytes");
                }
            } catch(SocketException e) {
                Error(e);
            }
        }

        public void SendMessage(IMessage message)
        {
            if(!Connected) {
                return;
            }

            NetworkMessage packet = new NetworkMessage();
            packet.Payload = message;

            _logger.Debug("Sending network message: " + packet);

            byte[] bytes = packet.Serialize(Formatter);
            _logger.Debug("Session " + Id + " sending " + bytes.Length + " bytes");
            _socketState.Send(bytes);
        }

        public bool HandleMessage(IMessageHandlerFactory factory, IMessage message)
        {
            if(HasMessageHandler && !_messageHandler.Finished) {
                _logger.Warn("Attempted to handle new message before handler completed!");
                return false;
            }

            _logger.Debug("Processing message with type=" + message.Type + " for session id=" + Id + "...");

            _messageHandler = factory.NewHandler(message.Type, this);
            if(null == _messageHandler) {
                return false;
            }

            _messageHandler.HandleMessage(message);
            return true;
        }

        public void Error(string error)
        {
            _logger.Error("Session " + Id + " encountered an error: " + error);
            Disconnect();
        }

        public void Error(string error, Exception ex)
        {
            _logger.Error("Session " + Id + " encountered an error: " + error, ex);
            Disconnect();
        }

        public void Error(Exception ex)
        {
            _logger.Error("Session " + Id + " encountered an error", ex);
            Disconnect();
        }

        public void SocketError(string error)
        {
            _logger.Error("Socket " + Id + " encountered an error: " + error);
            Disconnect();

            if(null != OnSocketError) {
                OnSocketError(error);
            }
        }

        public void SocketError(Exception error)
        {
            SocketError(error.Message);
        }
    }
}
