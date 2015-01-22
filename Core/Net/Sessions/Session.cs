using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Packet;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Properties;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.Net.Sessions
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
        public event EventHandler<DisconnectEventArgs> OnDisconnect;
        public event EventHandler<ErrorEventArgs> OnError;
#endregion

        public readonly int Id;

#region Message Properties
        public abstract IMessagePacketParser Parser { get; }
        public abstract string FormatterType { get; }
        protected abstract IMessageHandlerFactory HandlerFactory { get; }

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

            _socketState = new SocketState();

            Timeout = -1;

            Processor = new MessageProcessor(this);
            Processor.Start();
        }

        protected Session(Socket socket) : this()
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
                Processor.Dispose();
                _socketState.Dispose();
            }
        }
#endregion

        public async Task ConnectAsync(string host, int port, SocketType socketType, ProtocolType protocolType)
        {
            Logger.Info("Session " + Id + " connecting to " + host + ":" + port + "...");
            _socketState.Socket = await NetUtil.ConnectAsync(host, port, socketType, protocolType).ConfigureAwait(false);
        }

        public async Task ConnectMulticastAsync(IPAddress group, int port, int ttl)
        {
            Logger.Info("Session " + Id + " connecting to multicast group " + group + ":" + port + "...");
            _socketState.Socket = await NetUtil.ConnectMulticastAsync(group, port, ttl).ConfigureAwait(false);
        }

        public async Task DisconnectAsync()
        {
            await DisconnectAsync(string.Empty);
        }

        public async Task DisconnectAsync(string reason)
        {
            try {
                if(!Connected) {
                    return;
                }

                Logger.Info("Session " + Id + " disconnecting: " + reason);
                await _socketState.ShutdownDisconnectCloseAsync(false).ConfigureAwait(false);

                if(null != OnDisconnect) {
                    OnDisconnect(this, new DisconnectEventArgs() { Reason = reason });
                }

                Processor.Stop();
            } catch(SocketException e) {
                Logger.Error("Error disconnecting socket!", e);
            }
        }

        public async Task<int> PollAndReceiveAllAsync()
        {
            try {
                if(!Connected) {
                    return -1;
                }

                int count = await _socketState.PollAndReceiveAllAsync().ConfigureAwait(false);
                if(count > 0) {
                    Logger.Debug("Session " + Id + " read " + count + " bytes");
                }
                return count;
            } catch(SocketException e) {
                InternalErrorAsync(e).Wait();
                return -1;
            }
        }

        public async Task PollAndRunAsync()
        {
            int count = await PollAndReceiveAllAsync().ConfigureAwait(false);
            if(count < 0) {
                await DisconnectAsync(Resources.DisconnectSocketClosed).ConfigureAwait(false);
                return;
            }

            if(TimedOut) {
                Logger.Info("Session " + Id + " timed out!");
                await DisconnectAsync(Resources.DisconnectTimeout).ConfigureAwait(false);
                return;
            }

            if(Connected) {
                await RunAsync().ConfigureAwait(false);
            }
        }

        public async Task RunAsync()
        {
            try {
                await Processor.ParseMessagesAsync(_socketState.Buffer).ConfigureAwait(false);
                await OnRunAsync().ConfigureAwait(false);
            } catch(MessageException e) {
                InternalErrorAsync(Resources.ErrorParsingMessages, e).Wait();
            }
        }

        protected async virtual Task OnRunAsync()
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        private async Task<int> SendAsync(byte[] bytes)
        {
            Logger.Debug("Session " + Id + " sending " + bytes.Length + " bytes");
            return await _socketState.SendAsync(bytes).ConfigureAwait(false);
        }

        public async Task SendMessageAsync(IMessage message)
        {
            try {
                if(!Connected) {
                    return;
                }

                MessagePacket packet = Parser.Create();
                packet.Content = message;
                Logger.Debug("Sending packet: " + packet);

                using(MemoryStream buffer = new MemoryStream()) {
                    await packet.SerializeAsync(buffer, FormatterType).ConfigureAwait(false);
                    await SendAsync(buffer.ToArray()).ConfigureAwait(false);
                }
            } catch(SocketException e) {
                InternalErrorAsync(Resources.ErrorSendingMessage, e).Wait();
            } catch(MessageException e) {
                InternalErrorAsync(Resources.ErrorSendingMessage, e).Wait(); 
            }
        }

        public async Task HandleMessageAsync(IMessage message)
        {
            try {
                Logger.Debug("Handling message with type=" + message.Type + " for session " + Id + "...");

                MessageHandler messageHandler = HandlerFactory.Create(message.Type);
                if(null == messageHandler) {
                    await InternalErrorAsync(string.Format(Resources.ErrorCreatingMessageHandler, message.Type)).ConfigureAwait(false);
                    return;
                }

                await messageHandler.HandleMessageAsync(message, this).ConfigureAwait(false);
                Logger.Debug("Handler for message with type=" + message.Type + " for session " + Id + " took " + messageHandler.RuntimeMs + "ms to complete");
            } catch(MessageHandlerException e) {
                InternalErrorAsync(string.Format(Resources.ErrorHandlingMessage), e).Wait();
            } catch(Exception e) {
                InternalErrorAsync(string.Format(Resources.ErrorHandlingMessage), e).Wait();
            }
        }

#region Internal Errors
        public async Task InternalErrorAsync(string error)
        {
            Logger.Error("Session " + Id + " encountered an internal error: " + error);
            await DisconnectAsync(Resources.DisconnectInternalError).ConfigureAwait(false);

            if(null != OnError) {
                OnError(this, new ErrorEventArgs() { Error = error });
            }
        }

        public async Task InternalErrorAsync(string error, Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an internal error: " + error, ex);
            await DisconnectAsync(Resources.DisconnectInternalError).ConfigureAwait(false);

            if(null != OnError) {
                OnError(this, new ErrorEventArgs() { Error = error, Exception = ex });
            }
        }

        public async Task InternalErrorAsync(Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an internal error", ex);
            await DisconnectAsync(Resources.DisconnectInternalError).ConfigureAwait(false);

            if(null != OnError) {
                OnError(this, new ErrorEventArgs() { Exception = ex });
            }
        }
#endregion

#region Errors
        public async Task ErrorAsync(string error)
        {
            Logger.Error("Session " + Id + " encountered an error: " + error);
            await DisconnectAsync(error).ConfigureAwait(false);

            if(null != OnError) {
                OnError(this, new ErrorEventArgs() { Error = error });
            }
        }

        public async Task ErrorAsync(string error, Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an error: " + error, ex);
            await DisconnectAsync(error).ConfigureAwait(false);

            if(null != OnError) {
                OnError(this, new ErrorEventArgs() { Error = error, Exception = ex });
            }
        }

        public async Task ErrorAsync(Exception ex)
        {
            Logger.Error("Session " + Id + " encountered an error", ex);
            await DisconnectAsync(ex.Message).ConfigureAwait(false);

            if(null != OnError) {
                OnError(this, new ErrorEventArgs() { Exception = ex });
            }
        }
    }
#endregion
}
