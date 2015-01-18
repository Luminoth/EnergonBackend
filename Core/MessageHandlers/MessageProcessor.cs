using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Packet;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Properties;
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

        private CancellationTokenSource _cancellationToken;
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
                _cancellationToken.Dispose();
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
            MessagePacket packet = await _session.Parser.ParseAsync(buffer, _session.FormatterType).ConfigureAwait(false);
            while(null != packet) {
                Logger.Debug("Session " + _session.Id + " parsed message type: " + packet.Content.Type);
                QueueMessage(packet.Content);
                packet = await _session.Parser.ParseAsync(buffer, _session.FormatterType).ConfigureAwait(false);
            }
        }

        public void Start()
        {
            if(null != _task) {
                throw new InvalidOperationException(Resources.ErrorMessageProcessorAlreadyRunning);
            }

            Logger.Debug("Starting message processor for session " + _session.Id + "...");

            _cancellationToken = new CancellationTokenSource();
            _task = Task.Run(async () =>
                {
                    // TODO: trap exceptions
                    while(!_cancellationToken.IsCancellationRequested) {
                        _messageQueueEvent.WaitOne();
                        await RunAsync().ConfigureAwait(false);
                    }
                }, _cancellationToken.Token);
        }

        public void Stop()
        {
            if(null == _task || _cancellationToken.IsCancellationRequested) {
                return;
            }

            Logger.Debug("Stopping message processor for session " + _session.Id + "...");

            _cancellationToken.Cancel();
            _messageQueueEvent.Set();
            _task.Wait();

            _task = null;
            _cancellationToken = null;

            _messageQueue = new ConcurrentQueue<IMessage>();

            Logger.Debug("Message processor for session " + _session.Id + " finished!");
        }

        private async Task RunAsync()
        {
            IMessage message;
            while(!_cancellationToken.IsCancellationRequested && _messageQueue.TryDequeue(out message)) {
                await _session.HandleMessageAsync(message).ConfigureAwait(false);
            }
        }
    }
}
