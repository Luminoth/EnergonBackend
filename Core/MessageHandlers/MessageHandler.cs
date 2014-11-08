using System.Threading.Tasks;

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
private Task _task;
public bool Finished { get { return null != _task && _task.IsCompleted; } }
        //public bool Finished { get; private set; }

        private long _startTime, _finishTime;
        public long RuntimeMs { get { return Finished ? _finishTime - _startTime : Time.CurrentTimeMs - _startTime; } }

        public /*async*/ void HandleMessage(IMessage message)
        {
_task = Task.Factory.StartNew(() =>
    {
        _startTime = Time.CurrentTimeMs;
        OnHandleMessage(message);
        _finishTime = Time.CurrentTimeMs;
    }
);
            /*Finished = false;
            _startTime = Time.CurrentTimeMs;
            await OnHandleMessage(session, message);
            _finishTime = Time.CurrentTimeMs;
            Finished = true;*/
        }

        protected abstract void OnHandleMessage(IMessage message);
    }
}
