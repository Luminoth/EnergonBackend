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
        Session CreateSession(SessionManager manager);
        Session CreateSession(Socket socket, SessionManager manager);
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

        public delegate void OnDisconnectHandler(string reason);
        public event OnDisconnectHandler OnDisconnect;

        public delegate void OnErrorHandler(string error);
        public event OnErrorHandler OnError;
#endregion

        private readonly object _lock = new object();

        public readonly int Id;
        private readonly SessionManager _manager;

#region Network Properties
        private readonly SocketState _socketState;
        public EndPoint RemoteEndPoint { get { return _socketState.RemoteEndPoint ; } }
        public bool Blocking { get { return _socketState.Blocking; } set { _socketState.Blocking = value; } }

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

        private Session()
        {
            Id = NextId;

            _socketState = new SocketState();

            Timeout = -1;
        }

        public Session(SessionManager manager)
        {
            Id = NextId;
            _manager = manager;

            _socketState = new SocketState();

            Timeout = -1;
        }

        public Session(Socket socket, SessionManager manager)
        {
            Id = NextId;
            _manager = manager;

            _socketState = new SocketState(socket);

            Timeout = -1;
        }

        public void Run(MessageProcessor processor)
        {
            lock(_lock) {
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
        }

        protected abstract void OnRun();

        private void OnConnectAsyncFailedCallback(SocketError error)
        {
            _logger.Error("Session " + Id + " connect failed: " + error);

            Disconnect(error.ToString());

            if(null != OnConnectFailed) {
                OnConnectFailed(error);
            }
        }

        private void OnConnectAsyncSuccessCallback(Socket socket)
        {
            _logger.Info("Connected session " + Id + " to " + socket.RemoteEndPoint);

            lock(_lock) {
                _socketState.Socket = socket;
                _socketState.Blocking = _manager.Blocking;
                _socketState.Connecting = false;
            }

            if(null != OnConnectSuccess) {
                OnConnectSuccess();
            }
        }

        public void ConnectAsync(string host, int port)
        {
            _logger.Info("Session " + Id + " connecting to " + host + ":" + port + "...");

            lock(_lock) {
                _socketState.Connecting = true;
            }

            AsyncConnectEventArgs args = new AsyncConnectEventArgs();
            args.OnConnectFailed += OnConnectAsyncFailedCallback;
            args.OnConnectSuccess += OnConnectAsyncSuccessCallback;
            NetUtil.ConnectAsync(host, port, args);
        }


        public void Disconnect(string reason=null)
        {
            lock(_lock) {
                if(Connected) {
                    _logger.Info("Session " + Id + " disconnecting: " + reason);
                    _socketState.ShutdownAndClose(false);

                    if(null != OnDisconnect) {
                        OnDisconnect(reason);
                    }
                }
            }
        }

        public int PollAndRead()
        {
            lock(_lock) {
                if(!Connected) {
                    return -1;
                }

                try {
                    int count = _socketState.PollAndRead();
                    if(count > 0) {
                        _logger.Debug("Session " + Id + " read " + count + " bytes");
                    }
                    return count;
                } catch(SocketException e) {
                    Error(e);
                    return -1;
                }
            }
        }

        public void SendMessage(IMessage message)
        {
            lock(_lock) {
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
        }

        public bool HandleMessage(IMessageHandlerFactory factory, IMessage message)
        {
            lock(_lock) {
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
        }

        public void Error(string error)
        {
            _logger.Error("Session " + Id + " encountered an error: " + error);
            Disconnect(error);

            if(null != OnError) {
                OnError(error);
            }
        }

        public void Error(string error, Exception ex)
        {
            _logger.Error("Session " + Id + " encountered an error: " + error, ex);
            Disconnect(error);

            if(null != OnError) {
                OnError(error);
            }
        }

        public void Error(Exception ex)
        {
            _logger.Error("Session " + Id + " encountered an error", ex);
            Disconnect(ex.Message);

            if(null != OnError) {
                OnError(ex.Message);
            }
        }
    }
}
