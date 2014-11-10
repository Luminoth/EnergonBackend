using System;
using System.Threading.Tasks;

using log4net;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.MessageHandlers
{
    public interface IMessageHandlerFactory
    {
        MessageHandler NewHandler(string type, Session session);
    }

    public abstract class MessageHandler
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MessageHandler));

private Task _task;
public bool Finished { get { return null != _task && _task.IsCompleted; } }
        //public bool Finished { get; private set; }

        private long _startTime, _finishTime;
        public long RuntimeMs { get { return Finished ? _finishTime - _startTime : Time.CurrentTimeMs - _startTime; } }

        public /*async*/ void HandleMessage(IMessage message)
        {
_task = Task.Factory.StartNew(() =>
    {
        try {
            _startTime = Time.CurrentTimeMs;
            OnHandleMessage(message);
            _finishTime = Time.CurrentTimeMs;
        } catch(Exception e) {
            _logger.Error("Unhandled Exception!", e);
        }
    }
);
            /*Finished = false;
            _startTime = Time.CurrentTimeMs;
            await OnHandleMessage(message);
            _finishTime = Time.CurrentTimeMs;
            Finished = true;*/
        }

        protected abstract void OnHandleMessage(IMessage message);
    }
}
