using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Packet;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public interface ISessionFactory
    {
        Session Create(Socket socket);
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

#region Message Properties
        public abstract IMessagePacketParser Parser { get; }
        public abstract IMessageFormatter Formatter { get; }
        public abstract IMessageHandlerFactory HandlerFactory { get; }

        protected readonly MessageProcessor Processor;
#endregion

#region Network Properties
        private readonly SocketState _socketState;
        public EndPoint RemoteEndPoint { get { return _socketState.RemoteEndPoint; } }

        public bool Connecting { get { return _socketState.Connecting; } }
        public bool Connected { get { return _socketState.Connected; } }

        public long LastMessageTime { get { return _socketState.LastMessageTime; } }

        public long Timeout { get; set; }
        public bool TimedOut { get { return Timeout < 0 ? false : Time.CurrentTimeMs >= (_socketState.LastMessageTime + Timeout); } }
#endregion

        protected Session()
        {
            Id = NextId;

            Processor = new MessageProcessor(this);
            Processor.Start();

            _socketState = new SocketState();

            Timeout = -1;
        }

        public Session(Socket socket) : this()
        {
            _socketState = new SocketState(socket);
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
                Processor.Stop();
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
                    try {
                        Logger.Info("Session " + Id + " disconnecting: " + reason);
                        _socketState.ShutdownAndClose(false);

                        if(null != OnDisconnect) {
                            OnDisconnect(this, new DisconnectEventArgs() { Reason = reason });
                        }
                    } catch(SocketException e) {
                        Logger.Error("Error disconnecting socket!", e);
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

        public void Run()
        {
            lock(_lock) {
                try {
                    Processor.ParseMessages(_socketState.Buffer);

                    OnRun();
                } catch(MessageException e) {
                    Error("Exception while parsing messages!", e);
                }
            }
        }

        protected virtual void OnRun()
        {
        }

        public void SendMessage(IMessage message)
        {
            lock(_lock) {
                if(!Connected) {
                    return;
                }

                try {
                    MessagePacket packet = Parser.Create();
                    packet.Content = message;

                    Logger.Debug("Sending packet: " + packet);

                    byte[] bytes = packet.Serialize(Formatter);
                    Logger.Debug("Session " + Id + " sending " + bytes.Length + " bytes");
                    _socketState.Send(bytes);
                } catch(SocketException e) {
                    Error("Error sending message!", e);
                } catch(MessageException e) {
                    Error("Error sending message!", e);
                }
            }
        }

#region Internal Errors
        public void InternalError(string error)
        {
            Logger.Error("Session " + Id + " encountered an internal error: " + error);
            Disconnect("Internal Error");

            if(null != OnError) {
                OnError(this, new ErrorEventArgs() { Error = error });
            }
        }

        public void InternalError(string error, Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an internal error: " + error, ex);
            Disconnect("Internal Error");

            if(null != OnError) {
                OnError(this, new ErrorEventArgs() { Error = error, Exception = ex });
            }
        }

        public void InternalError(Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an internal error", ex);
            Disconnect("Internal Error");

            if(null != OnError) {
                OnError(this, new ErrorEventArgs() { Exception = ex });
            }
        }
#endregion

#region Errors
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
#endregion
}
