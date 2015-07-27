using System.Collections.Concurrent;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Net.Sessions;

namespace EnergonSoftware.Backend.MessageHandlers
{
    /// <summary>
    /// Queues and processes MessageNetworkSession MessageRecievedEvents
    /// </summary>
    public class MessageProcessor
    {
        private readonly ConcurrentQueue<MessageProcessorItem> _messageQueue = new ConcurrentQueue<MessageProcessorItem>();

        /// <summary>
        /// Message received event handler
        /// </summary>
        /// <param name="sender">The sender. This should be a MessageNetworkSession.</param>
        /// <param name="e">The <see cref="MessageReceivedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// This should be attached to the MessageReceievedEvent of a MessageNetworkSession
        /// </remarks>
        public void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e)
        {
            EnqueueMessage(sender as MessageNetworkSession, e.Message);
        }

        /// <summary>
        /// Enqueues a message for the given session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        public void EnqueueMessage(MessageNetworkSession session, Message message)
        {
            if(null == session || null == message) {
                return;
            }

            _messageQueue.Enqueue(new MessageProcessorItem
                {
                    Session = session,
                    Message = message
                }
            );
        }

        /// <summary>
        /// Runs the processor.
        /// </summary>
        public async Task RunAsync()
        {
            MessageProcessorItem messageItem;
            while(_messageQueue.TryDequeue(out messageItem)) {
                if(!messageItem.Session.IsConnected) {
                    continue;
                }

                MessageHandler messageHandler = messageItem.Session.MessageHandlerFactory.Create(messageItem.Message.Type);
                await messageHandler.HandleMessageAsync(messageItem.Message, messageItem.Session).ConfigureAwait(false);
            }
        }
    }
}
