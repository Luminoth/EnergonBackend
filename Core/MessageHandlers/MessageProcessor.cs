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

        public /*async Task*/ void ParseMessages(MemoryBuffer buffer)
        {
            MessagePacket packet = _session.Parser.Parse(buffer, _session.Formatter);
            while(null != packet) {
                Logger.Debug("Session " + _session.Id + " parsed message type: " + packet.Content.Type);
                QueueMessage(packet.Content);
                packet = _session.Parser.Parse(buffer, _session.Formatter);
            }
        }

        public /*async Task*/ void Start()
        {
            Logger.Debug("Starting message processor for session " + _session.Id + "...");

            _running = true;
_task = Task.Factory.StartNew(() => Run());
            //await Task.Run(() => Run());
        }

        public void Stop()
        {
            if(!_running) {
                return;
            }

            Logger.Debug("Stopping message processor for session " + _session.Id + "...");
            _running = false;
_task.Wait();

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

        private void HandleMessage(IMessage message)
        {
            if(null != _messageHandler && !_messageHandler.Finished) {
                _session.InternalError("Session " + _session.Id + " attempted to handle new message before handler completed!");
                return;
            }

            Logger.Debug("Processing message with type=" + message.Type + " for session " + _session.Id + "...");

            try {
                _messageHandler = _session.HandlerFactory.Create(message.Type);
                if(null == _messageHandler) {
                    _session.InternalError("Session " + _session.Id + " could not create handler for message type: " + message.Type);
                    return;
                }
                _messageHandler.HandleMessage(message, _session);
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
