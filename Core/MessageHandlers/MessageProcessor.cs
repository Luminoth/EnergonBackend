using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Packet;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.MessageHandlers
{
    public sealed class MessageProcessor : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageProcessor));

        private readonly Session _session;

        private readonly AutoResetEvent _messageQueueEvent = new AutoResetEvent(false);
        private /*readonly*/ ConcurrentQueue<IMessage> _messageQueue = new ConcurrentQueue<IMessage>();

        private volatile bool _running;
        private Task _task;

        private MessageProcessor()
        {
        }

        public MessageProcessor(Session session) : this()
        {
            _session = session;
        }

#region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(disposing) {
                _messageQueueEvent.Dispose();
            }
        }
#endregion

        public int GetQueueSize()
        {
            return _messageQueue.Count;
        }

        public void QueueMessage(IMessage message)
        {
            _messageQueue.Enqueue(message);
            _messageQueueEvent.Set();
        }

        public async Task ParseMessagesAsync(MemoryBuffer buffer)
        {
            MessagePacket packet = await _session.Parser.ParseAsync(buffer, _session.Formatter).ConfigureAwait(false);
            while(null != packet) {
                Logger.Debug("Session " + _session.Id + " parsed message type: " + packet.Content.Type);
                QueueMessage(packet.Content);
                packet = await _session.Parser.ParseAsync(buffer, _session.Formatter).ConfigureAwait(false);
            }
        }

        public void Start()
        {
            Logger.Debug("Starting message processor for session " + _session.Id + "...");

            _running = true;
            _task = Task.Run(() => Run());
        }

        public void Stop()
        {
            if(!_running) {
                return;
            }

            Logger.Debug("Stopping message processor for session " + _session.Id + "...");

            _running = false;
            _messageQueueEvent.Set();

            _task.Wait();
            _task = null;

            _messageQueue = new ConcurrentQueue<IMessage>();
        }

        private void Run()
        {
            RunAsync().Wait();
        }

        private async Task RunAsync()
        {
            while(_running) {
                _messageQueueEvent.WaitOne();

                IMessage message;
                while(_running && _messageQueue.TryDequeue(out message)) {
                    await HandleMessageAsync(message).ConfigureAwait(false);
                }
            }
        }

        private async Task HandleMessageAsync(IMessage message)
        {
            Logger.Debug("Processing message with type=" + message.Type + " for session " + _session.Id + "...");

            try {
                MessageHandler messageHandler = _session.HandlerFactory.Create(message.Type);
                if(null == messageHandler) {
                    _session.InternalError("Session " + _session.Id + " could not create handler for message type: " + message.Type);
                    return;
                }
                await messageHandler.HandleMessageAsync(message, _session).ConfigureAwait(false);
                Logger.Debug("Handler for message with type=" + message.Type + " for session " + _session.Id + " took " + messageHandler.RuntimeMs + "ms to complete");
            } catch(MessageHandlerException e) {
                _session.InternalError("Error handling message for session " + _session.Id, e);
            } catch(Exception e) {
                _session.InternalError("Unhandled message processing exception for session + " + _session.Id + "!", e);
            }
        }
    }
}
