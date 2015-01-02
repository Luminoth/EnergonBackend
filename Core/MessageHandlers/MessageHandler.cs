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
        MessageHandler Create(string type);
    }

    public class MessageHandler
    {
        public bool Finished { get; private set; }

        private long _startTime, _finishTime;
        public long RuntimeMs { get { return Finished ? _finishTime - _startTime : Time.CurrentTimeMs - _startTime; } }

        private void Authenticate(IAuthenticatedMessage message, AuthenticatedSession session)
        {
            if(null != message && null != session) {
                if(!session.Authenticate(message.Username, message.SessionId)) {
                    throw new MessageHandlerException("Session is not authenticated!");
                }
            }
        }

        public async Task HandleMessage(IMessage message, Session session)
        {
            Authenticate(message as IAuthenticatedMessage, session as AuthenticatedSession);

            Finished = false;
            _startTime = Time.CurrentTimeMs;
            await Task.Run(() => OnHandleMessage(message, session));
            _finishTime = Time.CurrentTimeMs;
            Finished = true;
        }

        protected virtual void OnHandleMessage(IMessage message, Session session)
        {
        }
    }
}
