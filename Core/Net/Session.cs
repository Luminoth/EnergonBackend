using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public interface ISessionFactory
    {
        Session CreateSession(SessionManager manager);
        Session CreateSession(Socket socket, SessionManager manager);
    }

    public abstract class Session : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Session));

#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

#region Events
        public delegate void OnConnectSuccessHandler(object sender, ConnectEventArgs e);
        public event OnConnectSuccessHandler OnConnectSuccess;
        
        public delegate void OnConnectFailedHandler(object sender, ConnectEventArgs e);
        public event OnConnectFailedHandler OnConnectFailed;

        public delegate void OnDisconnectHandler(object sender, DisconnectEventArgs e);
        public event OnDisconnectHandler OnDisconnect;

        public delegate void OnErrorHandler(object sender, ErrorEventArgs e);
        public event OnErrorHandler OnError;
#endregion

        private readonly object _lock = new object();

        public readonly int Id;
        private readonly SessionManager _manager;

#region Network Properties
        private readonly SocketState _socketState;
        public EndPoint RemoteEndPoint { get { return _socketState.RemoteEndPoint; } }

        public bool Connecting { get { return _socketState.Connecting; } }
        public bool Connected { get { return _socketState.Connected; } }

        public long LastMessageTime { get { return _socketState.LastMessageTime; } }

        public long Timeout { get; set; }
        public bool TimedOut { get { return Timeout < 0 ? false : Time.CurrentTimeMs >= (_socketState.LastMessageTime + Timeout); } }
#endregion

#region Message Properties
        private MessageHandler _messageHandler;
        public bool HasMessageHandler { get { return null != _messageHandler; } }
        private MessageHandler MessageHandler { get { return _messageHandler; } set { _messageHandler = value; } }

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

#region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _socketState.Dispose();
            }
        }
#endregion

        private void OnConnectAsyncFailedCallback(object sender, ConnectEventArgs e)
        {
            Logger.Error("Session " + Id + " connect failed: " + e.Error);

            Disconnect(e.Error.ToString());

            if(null != OnConnectFailed) {
                OnConnectFailed(sender, e);
            }
        }

        private void OnConnectAsyncSuccessCallback(object sender, ConnectEventArgs e)
        {
            Logger.Info("Connected session " + Id + " to " + e.Socket.RemoteEndPoint);

            lock(_lock) {
                _socketState.Socket = e.Socket;
                _socketState.Connecting = false;
            }

            if(null != OnConnectSuccess) {
                OnConnectSuccess(sender, e);
            }
        }

        public void ConnectAsync(string host, int port)
        {
            Logger.Info("Session " + Id + " connecting to " + host + ":" + port + "...");

            lock(_lock) {
                _socketState.Connecting = true;
            }

            AsyncConnectEventArgs args = new AsyncConnectEventArgs()
            {
                Sender = this,
            };
            args.OnConnectFailed += OnConnectAsyncFailedCallback;
            args.OnConnectSuccess += OnConnectAsyncSuccessCallback;
            NetUtil.ConnectAsync(host, port, args);
        }

        public void Connect(string host, int port, SocketType socketType, ProtocolType protocolType)
        {
            Logger.Info("Session " + Id + " connecting to " + host + ":" + port + "...");

            lock(_lock) {
                _socketState.Socket = NetUtil.Connect(host, port, socketType, protocolType);
            }
        }

        public void ConnectMulticast(IPAddress group, int port, int ttl)
        {
            Logger.Info("Session " + Id + " connecting to multicast group " + group + ":" + port + "...");

            lock(_lock) {
                _socketState.Socket = NetUtil.ConnectMulticast(group, port, ttl);
            }
        }

        public void Disconnect(string reason=null)
        {
            lock(_lock) {
                if(Connected) {
                    Logger.Info("Session " + Id + " disconnecting: " + reason);
                    _socketState.ShutdownAndClose(false);

                    if(null != OnDisconnect) {
                        OnDisconnect(this, new DisconnectEventArgs() { Reason = reason });
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
                        Logger.Debug("Session " + Id + " read " + count + " bytes");
                    }
                    return count;
                } catch(SocketException e) {
                    Error(e);
                    return -1;
                }
            }
        }

        public void BufferWrite(byte[] data, int offset, int count)
        {
            lock(_lock) {
                _socketState.Buffer.Write(data, offset, count);
            }
        }

        public void Run(MessageProcessor processor, IMessageParser parser)
        {
            lock(_lock) {
                processor.ParseMessages(this, parser, _socketState.Buffer, Formatter);

                // TODO: we need a way to say "hey, this handler is taking WAY too long,
                // dump an error and kill the session"
                if(HasMessageHandler && MessageHandler.Finished) {
                    MessageHandler = null;
                }

                OnRun(processor, parser);
            }
        }

        protected virtual void OnRun(MessageProcessor processor, IMessageParser parser)
        {
        }

        public void SendMessage(IMessage message)
        {
            lock(_lock) {
                if(!Connected) {
                    return;
                }

                NetworkMessage packet = new NetworkMessage();
                packet.Payload = message;

                Logger.Debug("Sending network message: " + packet);

                byte[] bytes = packet.Serialize(Formatter);
                Logger.Debug("Session " + Id + " sending " + bytes.Length + " bytes");
                _socketState.Send(bytes);
            }
        }

        public bool HandleMessage(IMessageHandlerFactory factory, IMessage message)
        {
            lock(_lock) {
                if(HasMessageHandler && !_messageHandler.Finished) {
                    Logger.Warn("Attempted to handle new message before handler completed!");
                    return false;
                }

                Logger.Debug("Processing message with type=" + message.Type + " for session id=" + Id + "...");

                try {
                    _messageHandler = factory.NewHandler(message.Type);
                    if(null == _messageHandler) {
                        return false;
                    }
                    _messageHandler.HandleMessage(message, this);
                } catch(MessageHandlerException e) {
                    Logger.Error("Error handling message", e);
                    return false;
                } catch(Exception e) {
                    Logger.Error("Unhandled message processing exception!", e);
                    return false;
                }
                return true;
            }
        }

        public void Error(string error)
        {
            Logger.Error("Session " + Id + " encountered an error: " + error);
            Disconnect(error);

            if(null != OnError) {
                OnError(this, new ErrorEventArgs() { Error = error });
            }
        }

        public void Error(string error, Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an error: " + error, ex);
            Disconnect(error);

            if(null != OnError) {
                OnError(this, new ErrorEventArgs() { Error = error, Exception = ex });
            }
        }

        public void Error(Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an error", ex);
            Disconnect(ex.Message);

            if(null != OnError) {
                OnError(this, new ErrorEventArgs() { Exception = ex });
            }
        }
    }
}
