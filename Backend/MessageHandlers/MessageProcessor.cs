using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Parser;
using EnergonSoftware.Backend.Properties;

using log4net;

namespace EnergonSoftware.Backend.MessageHandlers
{
    /// <summary>
    /// Processes messages
    /// </summary>
    public sealed class MessageProcessor : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageProcessor));

        private readonly AutoResetEvent _messageQueueEvent = new AutoResetEvent(false);
        private /*readonly*/ ConcurrentQueue<Message> _messageQueue = new ConcurrentQueue<Message>();

#region Events
        /// <summary>
        /// Occurs when a new message is available to be handled.
        /// </summary>
        public event EventHandler<HandleMessageEventArgs> HandleMessageEvent;
#endregion

        public int QueueSize => _messageQueue.Count;

        private CancellationTokenSource _cancellationToken;
        private Task _task;

#region Dispose
        public void Dispose()
        {
            Dispose(true);
            ////GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(disposing) {
                _messageQueueEvent.Dispose();
                _cancellationToken.Dispose();
            }
        }
#endregion

#region Event Handlers
        public void MessageParsedEventHandler(object sender, MessageParsedEventArgs e)
        {
            Logger.Debug($"Queueing message: {e.Message}");

            _messageQueue.Enqueue(e.Message);
            _messageQueueEvent.Set();
        }
#endregion

        public void QueueMessage(Message message)
        {
            _messageQueue.Enqueue(message);
            _messageQueueEvent.Set();
        }

        public void Start()
        {
            if(null != _task) {
                throw new InvalidOperationException(Resources.ErrorMessageProcessorAlreadyRunning);
            }

            Logger.Debug("Starting message processor...");

            _cancellationToken = new CancellationTokenSource();
            _task = Task.Run(
                async () =>
                {
                    // TODO: trap exceptions
                    while(!_cancellationToken.IsCancellationRequested) {
                        _messageQueueEvent.WaitOne();
                        if(_cancellationToken.IsCancellationRequested) {
                            break;
                        }

                        await RunAsync().ConfigureAwait(false);
                    }
                });
        }

        public void Stop()
        {
            if(null == _task || _cancellationToken.IsCancellationRequested) {
                return;
            }

            Logger.Debug("Stopping message processor...");

            _cancellationToken.Cancel();
            _messageQueueEvent.Set();
            _task.Wait();

            _task = null;
            _cancellationToken = null;

            _messageQueue = new ConcurrentQueue<Message>();

            Logger.Debug("Message processor finished!");
        }

        private async Task RunAsync()
        {
            Message message;
            while(!_cancellationToken.IsCancellationRequested && _messageQueue.TryDequeue(out message)) {
                HandleMessageEvent?.Invoke(this, new HandleMessageEventArgs { Message = message });
                await Task.Delay(0).ConfigureAwait(false);
            }
        }
    }
}
