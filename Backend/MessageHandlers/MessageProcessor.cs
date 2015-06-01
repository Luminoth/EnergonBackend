﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Parser;
using EnergonSoftware.Backend.Packet;
using EnergonSoftware.Backend.Properties;

using EnergonSoftware.Core.IO;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Backend.MessageHandlers
{
    public sealed class MessageProcessor : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageProcessor));

        private readonly AutoResetEvent _messageQueueEvent = new AutoResetEvent(false);
        private /*readonly*/ ConcurrentQueue<IMessage> _messageQueue = new ConcurrentQueue<IMessage>();

#region Events
        public event EventHandler<HandleMessageEventArgs> HandleMessageEvent;
#endregion

        public int QueueSize { get { return _messageQueue.Count; } }

        private CancellationTokenSource _cancellationToken;
        private Task _task;

        public MessageProcessor()
        {
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

#region Event Handlers
        public void MessageParsedEventHandler(object sender, MessageParsedEventArgs e)
        {
            Logger.Debug("Queueing message: " + e.Message.ToString());

            _messageQueue.Enqueue(e.Message);
            _messageQueueEvent.Set();
        }
#endregion

        public void QueueMessage(IMessage message)
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
                        await RunAsync().ConfigureAwait(false);
                    }
                },
                _cancellationToken.Token);
        }

        public void Stop()
        {
            if(null == _task || _cancellationToken.IsCancellationRequested) {
                return;
            }

            Logger.Debug("Stopping message processor...");

            _cancellationToken.Cancel();
            _messageQueueEvent.Set();
            try {
                _task.Wait();
            } catch(AggregateException e) {
                if(e.InnerException is TaskCanceledException) {
                    // ignore this
                } else {
                    throw;
                }
            }

            _task = null;
            _cancellationToken = null;

            _messageQueue = new ConcurrentQueue<IMessage>();

            Logger.Debug("Message processor finished!");
        }

        private async Task RunAsync()
        {
            IMessage message;
            while(!_cancellationToken.IsCancellationRequested && _messageQueue.TryDequeue(out message)) {
                if(null != HandleMessageEvent) {
                    HandleMessageEvent(this, new HandleMessageEventArgs() { Message = message });
                }

                await Task.Delay(0).ConfigureAwait(false);
            }
        }
    }
}