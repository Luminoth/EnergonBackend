using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using log4net;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;

namespace EnergonSoftware.Core.MessageHandlers
{
    public sealed class MessageProcessor
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MessageProcessor));

        private class MessageQueueContext
        {
            public Session Session { get; set; }
            public IMessage Message { get; set; }
        }

        private ConcurrentDictionary<int, ConcurrentQueue<MessageQueueContext>> _messageQueue = new ConcurrentDictionary<int,ConcurrentQueue<MessageQueueContext>>();
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

        public bool RemoveSession(int sessionId)
        {
            ConcurrentQueue<MessageQueueContext> queue;
            return _messageQueue.TryRemove(sessionId, out queue);
        }

        public /*async*/ void Start(IMessageHandlerFactory factory)
        {
            _logger.Debug("Starting message processor...");
            _factory = factory;

            _running = true;
_task = Task.Factory.StartNew(() =>
    {
        Run();
    }
);
            //await Run();
        }

        public void Stop()
        {
            if(!_running) {
                return;
            }

            _logger.Debug("Stopping message processor...");
            _running = false;

            _task.Wait();

            _logger.Debug("Clearing message queue...");
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
