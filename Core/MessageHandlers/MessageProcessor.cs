using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.MessageHandlers
{
/*
 * TODO: move the queue into the Session
 * give the processor the SessionManager
 * for each session in the manager
 *  check for a handler, if there isn't one and a message is waiting, spin a new one
 */

// TODO: reevaluate if this class is even necessary
// why doesn't each Session just handle it's shit?

    public sealed class MessageProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageProcessor));

        private class MessageQueueContext
        {
            public Session Session { get; set; }
            public IMessage Message { get; set; }
        }

        private ConcurrentDictionary<int, ConcurrentQueue<MessageQueueContext>> _messageQueue = new ConcurrentDictionary<int, ConcurrentQueue<MessageQueueContext>>();
        private IMessageHandlerFactory _factory;

private Task _task;
        private volatile bool _running;

        public MessageProcessor()
        {
        }

        public int GetQueueSize(int sessionId)
        {
            ConcurrentQueue<MessageQueueContext> queue;
            if(_messageQueue.TryGetValue(sessionId, out queue)) {
                return queue.Count;
            }
            return 0;
        }

        public bool QueueMessage(Session session, IMessage message)
        {
            _messageQueue.TryAdd(session.Id, new ConcurrentQueue<MessageQueueContext>());

            ConcurrentQueue<MessageQueueContext> queue;
            if(_messageQueue.TryGetValue(session.Id, out queue)) {
                queue.Enqueue(new MessageQueueContext()
                    {
                        Session = session,
                        Message = message,
                    }
                );
                return true;
            }
            return false;
        }

        public void ParseMessages(Session session, IMessageParser parser, MemoryBuffer buffer, IMessageFormatter formatter)
        {
            MessagePacket packet = parser.Parse(buffer, formatter);
            while(null != packet) {
                Logger.Debug("Session " + session.Id + " parsed message type: " + packet.Payload.Type);
                QueueMessage(session, packet.Payload);
                packet = parser.Parse(buffer, formatter);
            }
        }

        public bool RemoveSession(int sessionId)
        {
            ConcurrentQueue<MessageQueueContext> queue;
            return _messageQueue.TryRemove(sessionId, out queue);
        }

        public /*async Task*/ void Start(IMessageHandlerFactory factory)
        {
            Logger.Debug("Starting message processor...");
            _factory = factory;

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
            _messageQueue.Clear();
            _factory = null;
_task = null;
        }

        private void Run()
        {
            while(_running) {
                foreach(var sessionQueue in _messageQueue) {
                    MessageQueueContext context;
                    if(sessionQueue.Value.TryPeek(out context)) {
                        if(context.Session.HasMessageHandler) {
                            continue;
                        }

                        if(sessionQueue.Value.TryDequeue(out context)) {
                            context.Session.HandleMessage(_factory, context.Message);
                        }
                    }
                }

                Thread.Sleep(0);
            }

            // TODO: cleanup any currently running handlers
        }
    }
}
