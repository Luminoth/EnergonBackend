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
    public sealed class MessageProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageProcessor));

        private readonly Session _session;

        private /*readonly*/ ConcurrentQueue<IMessage> _messageQueue = new ConcurrentQueue<IMessage>();
        private readonly AutoResetEvent _messageQueueEvent = new AutoResetEvent(false);

        private volatile bool _running;
        private Task _task;

        private MessageProcessor()
        {
        }

        public MessageProcessor(Session session) : this()
        {
            _session = session;
        }

        public int GetQueueSize()
        {
            return _messageQueue.Count;
        }

        public void QueueMessage(IMessage message)
        {
            _messageQueue.Enqueue(message);
            _messageQueueEvent.Set();
        }

        public async Task ParseMessages(MemoryBuffer buffer)
        {
            MessagePacket packet = await Task.Run(() => _session.Parser.Parse(buffer, _session.Formatter));
            while(null != packet) {
                Logger.Debug("Session " + _session.Id + " parsed message type: " + packet.Content.Type);
                QueueMessage(packet.Content);
                packet = await Task.Run(() => _session.Parser.Parse(buffer, _session.Formatter));
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

        private async Task Run()
        {
            while(_running) {
                _messageQueueEvent.WaitOne();

                IMessage message;
                while(_running && _messageQueue.TryDequeue(out message)) {
                    await HandleMessage(message);
                }
            }
        }

        private async Task HandleMessage(IMessage message)
        {
            Logger.Debug("Processing message with type=" + message.Type + " for session " + _session.Id + "...");

            try {
                MessageHandler messageHandler = _session.HandlerFactory.Create(message.Type);
                if(null == messageHandler) {
                    _session.InternalError("Session " + _session.Id + " could not create handler for message type: " + message.Type);
                    return;
                }
                await messageHandler.HandleMessage(message, _session);
            } catch(MessageHandlerException e) {
                _session.InternalError("Error handling message for session " + _session.Id, e);
                return;
            } catch(Exception e) {
                _session.InternalError("Unhandled message processing exception for session + " + _session.Id + "!", e);
                return;
            }
        }
    }
}
