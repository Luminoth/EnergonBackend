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
// change this to be owned by the session
// so do we need to even hold a reference back to the
// session with *each* message? or just hold it in the processor?

    public sealed class MessageProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageProcessor));

        private readonly Session _session;
        private ConcurrentQueue<IMessage> _messageQueue = new ConcurrentQueue<IMessage>();

        private MessageHandler _messageHandler;

private Task _task;
        private volatile bool _running;

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
        }

        public void ParseMessages(MemoryBuffer buffer)
        {
            MessagePacket packet = _session.Parser.Parse(buffer, _session.Formatter);
            while(null != packet) {
                Logger.Debug("Session " + _session.Id + " parsed message type: " + packet.Payload.Type);
                QueueMessage(packet.Payload);
                packet = _session.Parser.Parse(buffer, _session.Formatter);
            }
        }

        public /*async Task*/ void Start()
        {
            Logger.Debug("Starting message processor...");

            _running = true;
_task = Task.Factory.StartNew(() => Run());
            //await Task.Run(() => Run());
        }

        public void Stop()
        {
            if(!_running) {
                return;
            }

            Logger.Debug("Stopping message processor...");
            _running = false;
_task.Wait();

            Logger.Debug("Clearing message queue...");
            _messageQueue = new ConcurrentQueue<IMessage>();
_task = null;
        }

        private void Run()
        {
            while(_running) {
                // TODO: we need a way to say "hey, this handler is taking WAY too long,
                // dump an error and kill the session"
                if(null != _messageHandler && _messageHandler.Finished) {
                    _messageHandler = null;
                }

                IMessage message;
                if(null == _messageHandler && _messageQueue.TryDequeue(out message)) {
                    HandleMessage(message);
                }

                Thread.Sleep(0);
            }

            // TODO: cleanup any currently running handlers
        }

        private bool HandleMessage(IMessage message)
        {
            if(null != _messageHandler && !_messageHandler.Finished) {
                Logger.Warn("Attempted to handle new message before handler completed!");
                return false;
            }

            Logger.Debug("Processing message with type=" + message.Type + " for session id=" + _session.Id + "...");

            try {
                _messageHandler = _session.HandlerFactory.Create(message.Type);
                if(null == _messageHandler) {
                    return false;
                }
                _messageHandler.HandleMessage(message, _session);
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
}
