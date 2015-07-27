using System;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Net.Sessions;
using EnergonSoftware.Backend.Properties;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Backend.MessageHandlers
{
    /// <summary>
    /// Handles messages
    /// </summary>
    public class MessageHandler
    {
        private bool _running;

        /// <summary>
        /// Gets a value indicating whether this <see cref="MessageHandler"/> is finished.
        /// </summary>
        /// <value>
        ///   <c>true</c> if finished; otherwise, <c>false</c>.
        /// </value>
        public bool Finished { get; private set; }

        private DateTime _startTime, _finishTime;

        /// <summary>
        /// Gets the handler runtime.
        /// </summary>
        /// <value>
        /// The runtime.
        /// </value>
        public TimeSpan Runtime => Finished ? _finishTime.Subtract(_startTime) : DateTime.Now.Subtract(_startTime);

        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        /// <exception cref="MessageHandlerException"></exception>
        public async Task HandleMessageAsync(Message message, MessageNetworkSession session)
        {
            if(_running) {
                throw new MessageHandlerException(Resources.ErrorMessageHandlerAlreadyRunning);
            }

            _running = true;

            Authenticate(message as AuthenticatedMessage, session as AuthenticatedNetworkSession);

            Finished = false;
            _startTime = DateTime.Now;

            await OnHandleMessageAsync(message, session).ConfigureAwait(false);

            _finishTime = DateTime.Now;
            Finished = true;
        }

        private static void Authenticate(AuthenticatedMessage message, AuthenticatedNetworkSession session)
        {
            if(null != message && null != session) {
                if(!session.Authenticate(message.AccountName, message.SessionId)) {
                    throw new MessageHandlerException(Resources.ErrorSessionNotAuthenticated);
                }
            }
        }

        /// <summary>
        /// Called when a message is handled.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        protected async virtual Task OnHandleMessageAsync(Message message, NetworkSession session)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}
