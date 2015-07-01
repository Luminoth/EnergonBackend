using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Net.Sessions;
using EnergonSoftware.Backend.Properties;

using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Backend.MessageHandlers
{
    public interface IMessageHandlerFactory
    {
        MessageHandler Create(string type);
    }

    public class MessageHandler
    {
        private bool _running;
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

        private static void Authenticate(IAuthenticatedMessage message, AuthenticatedSession session)
        {
            if(null != message && null != session) {
                if(!session.Authenticate(message.AccountName, message.SessionId)) {
                    throw new MessageHandlerException(Resources.ErrorSessionNotAuthenticated);
                }
            }
        }

        protected async virtual Task OnHandleMessageAsync(IMessage message, NetworkSession session)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}
