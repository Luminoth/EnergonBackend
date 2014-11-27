using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.MessageHandlers
{
    public interface IMessageHandlerFactory
    {
        MessageHandler NewHandler(string type, Session session);
    }

    public class MessageHandler
    {
private Task _task;
public bool Finished { get { return null != _task && _task.IsCompleted; } }
        //public bool Finished { get; private set; }

        /*private long _startTime, _finishTime;
        public long RuntimeMs { get { return Finished ? _finishTime - _startTime : Time.CurrentTimeMs - _startTime; } }*/

        public /*async Task*/ void HandleMessage(IMessage message)
        {
_task = Task.Factory.StartNew(() => OnHandleMessage(message));
            /*Finished = false;
            _startTime = Time.CurrentTimeMs;
            await Task.Run(() => OnHandleMessage(message));
            _finishTime = Time.CurrentTimeMs;
            Finished = true;*/
        }

        protected virtual void OnHandleMessage(IMessage message)
        {
        }
    }
}
