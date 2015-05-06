using System.Threading.Tasks;

using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Properties;
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
        private static void Authenticate(IAuthenticatedMessage message, AuthenticatedSession session)
        {
            if(null != message && null != session) {
                if(!session.Authenticate(message.AccountName, message.SessionId)) {
                    throw new MessageHandlerException(Resources.ErrorSessionNotAuthenticated);
                }
            }
        }

        private bool _running = false;
        public bool Finished { get; private set; }

        private long _startTime, _finishTime;
        public long RuntimeMs { get { return Finished ? _finishTime - _startTime : Time.CurrentTimeMs - _startTime; } }

        public async Task HandleMessageAsync(IMessage message, NetworkSession session)
        {
            if(_running) {
                throw new MessageHandlerException(Resources.ErrorMessageHandlerAlreadyRunning);
            }

            _running = true;

            Authenticate(message as IAuthenticatedMessage, session as AuthenticatedSession);

            Finished = false;
            _startTime = Time.CurrentTimeMs;

            await OnHandleMessageAsync(message, session).ConfigureAwait(false);

            _finishTime = Time.CurrentTimeMs;
            Finished = true;
        }

        protected async virtual Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}
